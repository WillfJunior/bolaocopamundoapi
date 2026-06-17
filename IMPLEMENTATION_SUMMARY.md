# Resumo de Implementação - Classificação em Tempo Real e Ranking Dinâmico

## 📋 O que foi implementado

### ✅ 1. Serviço de Classificação dos Grupos da Copa (`GroupStandingService`)

**Arquivo:** `Application/Services/GroupStandingService.cs`

**Funcionalidades:**
- Calcula a classificação de cada grupo automaticamente baseada nos resultados finalizados
- Critérios de desempate implementados corretamente:
  1. **Pontos** (3 vitória, 1 empate, 0 derrota)
  2. **Saldo de gols** (gols marcados - gols sofridos)
  3. **Gols marcados** (quantidade total)
  4. **Ordem alfabética** (como último critério)

- Cache inteligente: Classificações são cacheadas por 30 segundos
- Cache é invalidado quando um jogo é finalizado
- Suporta consulta de grupo único ou todos os grupos

**Métodos:**
```csharp
public async Task<GroupStandingDto> GetGroupStandingAsync(string groupName)
public async Task<List<GroupStandingDto>> GetAllGroupStandingsAsync()
public void InvalidateGroupCache(string groupName)
```

---

### ✅ 2. Serviço de Ranking em Tempo Real (`BolaoGroupService`)

**Arquivo:** `Application/Services/BolaoGroupService.cs`

**Funcionalidades:**
- Calcula ranking em tempo real com pontos momentâneos
- Mostra a posição dinâmica que o jogador ficaria com os pontos atuais
- Calcula movimento de posição (↑/↓)
- Identifica o líder momentâneo
- Calcula diferença de pontos para o líder

**Novo método:**
```csharp
public async Task<List<RealTimeRankingEntryDto>> GetRealTimeGroupRankingAsync(Guid groupId, Guid userId)
```

**Algoritmo:**
1. Obtém ranking finalizado do grupo
2. Busca todos os jogos em andamento ou terminados
3. Para cada usuário, calcula pontos momentâneos
4. Recalcula posição com base em pontos momentâneos
5. Retorna ranking ordenado pela posição momentânea

---

### ✅ 3. DTOs Novos

**Arquivo:** `Application/DTOs/Match/TeamStandingDto.cs`
```csharp
public record TeamStandingDto(
    int TeamId,
    string TeamName,
    string FifaCode,
    string? FlagUrl,
    int Position,
    int Played,
    int Won,
    int Drawn,
    int Lost,
    int GoalsFor,
    int GoalsAgainst,
    int GoalDifference,
    int Points
);
```

**Arquivo:** `Application/DTOs/Match/GroupStandingDto.cs`
```csharp
public record GroupStandingDto(
    string GroupName,
    List<TeamStandingDto> Teams,
    List<MatchDto> Matches
);
```

**Arquivo:** `Application/DTOs/Ranking/RealTimeRankingEntryDto.cs`
```csharp
public record RealTimeRankingEntryDto(
    int Position,
    Guid UserId,
    string UserName,
    string? UserPhotoUrl,
    int TotalPoints,
    int ExactScores,
    int CorrectOutcomes,
    int TotalPredictions,
    int Errors,
    int MomentaryPoints,        // Pontos ganhando agora
    int MomentaryPosition,      // Posição se parar agora
    int PositionChange,         // Movimento (↑/↓)
    bool IsLeader,              // É líder momentâneo?
    int PointsDifference,       // Diferença para líder
    DateTime UpdatedAt
);
```

---

### ✅ 4. Controllers Atualizados

#### `GroupsController` - Novos Endpoints

**Arquivo:** `Controllers/GroupsController.cs`

```csharp
[HttpGet("{name}/standings")]
public async Task<ActionResult<GroupStandingDto>> GetGroupStanding(string name)
    => Ok(await standingService.GetGroupStandingAsync(name));

[HttpGet("standings/all")]
public async Task<ActionResult<List<GroupStandingDto>>> GetAllStandings()
    => Ok(await standingService.GetAllGroupStandingsAsync());
```

#### `RankingController` - Novo Endpoint

**Arquivo:** `Controllers/RankingController.cs`

```csharp
[HttpGet("real-time/{groupId:guid}")]
public async Task<ActionResult<List<RealTimeRankingEntryDto>>> GetRealTimeGroupRanking(Guid groupId)
{
    var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    return Ok(await bolaoGroupService.GetRealTimeGroupRankingAsync(groupId, userId));
}
```

---

### ✅ 5. Hub SignalR Melhorado

**Arquivo:** `Infrastructure/Hubs/RankingHub.cs`

**Métodos de Cliente → Servidor:**
```csharp
public async Task JoinGroupRanking(Guid groupId)
public async Task LeaveGroupRanking(Guid groupId)
public async Task JoinGlobalRanking()
public async Task LeaveGlobalRanking()
```

**Eventos de Servidor → Cliente:**
- `group-ranking-updated` - Ranking de um grupo foi atualizado
- `global-ranking-updated` - Ranking global foi atualizado
- `rankings-updated` - Compatibilidade com código existente

---

### ✅ 6. Atualização do PredictionService

**Arquivo:** `Application/Services/PredictionService.cs`

**Melhoria em `ProcessMatchPredictionsAsync`:**
```csharp
// Quando um jogo é finalizado, envia notificações via SignalR:
await hubContext.Clients.Group($"group-ranking-{groupId}")
    .SendAsync("group-ranking-updated", groupId, matchId);

await hubContext.Clients.Group("global-ranking")
    .SendAsync("global-ranking-updated", matchId);
```

---

### ✅ 7. Injeção de Dependência

**Arquivo:** `Program.cs`

```csharp
builder.Services.AddScoped<GroupStandingService>();
```

---

## 📊 Endpoints Disponíveis

### Classificação da Copa

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/groups/{name}/standings` | Classificação de um grupo (ex: "A") |
| GET | `/api/groups/standings/all` | Todas as classificações |

**Exemplo de Response:**
```json
{
  "groupName": "A",
  "teams": [
    {
      "teamId": 1,
      "teamName": "Brasil",
      "position": 1,
      "played": 3,
      "won": 3,
      "drawn": 0,
      "lost": 0,
      "goalsFor": 9,
      "goalsAgainst": 1,
      "goalDifference": 8,
      "points": 9
    }
  ],
  "matches": []
}
```

### Ranking em Tempo Real

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/ranking/real-time/{groupId}` | Ranking com pontos momentâneos |

**Exemplo de Response:**
```json
[
  {
    "position": 1,
    "userName": "João Silva",
    "totalPoints": 45,
    "momentaryPoints": 3,
    "momentaryPosition": 1,
    "positionChange": 0,
    "isLeader": true,
    "pointsDifference": 0
  }
]
```

---

## 🔄 Fluxo de Atualização em Tempo Real

```
┌─────────────────────────────────────────────────────────────┐
│ 1. JOGO INICIA                                              │
│    - Status: Scheduled → InProgress                         │
│    - SignalR: Nenhuma atualização                          │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ 2. RESULTADO ATUALIZADO (Admin Panel)                       │
│    - PATCH /api/admin/matches/{id}/result                  │
│    - HomeScore: null → 2, AwayScore: null → 1              │
│    - Status: InProgress                                     │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ 3. PROCESSAMENTO EM BACKGROUND                              │
│    - Hangfire processa predictions                          │
│    - Calcula pontos momentâneos                             │
│    - Invalida cache                                         │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ 4. NOTIFICAÇÃO VIA SIGNALR                                  │
│    - Evento: group-ranking-updated (groupId, matchId)      │
│    - Enviado para: Group($"group-ranking-{groupId}")       │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ 5. FRONTEND RECEBE EVENTO                                   │
│    - connection.on("group-ranking-updated", ...)            │
│    - Faz fetch: GET /api/ranking/real-time/{groupId}       │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ 6. UI EXIBE DADOS DINÂMICOS                                │
│    ✅ Pontos momentâneos em verde                           │
│    ✅ Posição dinâmica                                      │
│    ✅ Movimento de posição (↑/↓)                            │
│    ✅ Indicador de liderança (👑)                           │
│    ✅ Diferença de pontos                                   │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ 7. JOGO TERMINA                                             │
│    - PATCH /api/admin/matches/{id}/result (finalizado)     │
│    - Status: InProgress → Finished                          │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ 8. RANKINGS FINALIZADOS                                     │
│    - Pontos se tornam definitivos                           │
│    - Cache atualizado                                       │
│    - SignalR notifica atualização final                     │
└─────────────────────────────────────────────────────────────┘
```

---

## 🎯 Exemplo de Uso no Frontend

### React com SignalR

```typescript
import { useEffect, useState } from 'react';
import * as signalR from '@microsoft/signalr';

export function RealTimeRanking({ groupId }: { groupId: string }) {
  const [ranking, setRanking] = useState([]);
  const token = localStorage.getItem('token');

  useEffect(() => {
    // Conectar ao Hub
    const connection = new signalR.HubConnectionBuilder()
      .withUrl('https://api.example.com/hubs/ranking', {
        accessTokenFactory: () => token
      })
      .build();

    connection.start().then(() => {
      connection.invoke('JoinGroupRanking', groupId);
    });

    // Escutar atualizações
    connection.on('group-ranking-updated', () => {
      // Refetch do ranking em tempo real
      fetch(`/api/ranking/real-time/${groupId}`, {
        headers: { 'Authorization': `Bearer ${token}` }
      })
      .then(r => r.json())
      .then(data => setRanking(data));
    });

    // Fetch inicial
    fetch(`/api/ranking/real-time/${groupId}`, {
      headers: { 'Authorization': `Bearer ${token}` }
    })
    .then(r => r.json())
    .then(data => setRanking(data));

    return () => {
      connection.invoke('LeaveGroupRanking', groupId);
      connection.stop();
    };
  }, [groupId, token]);

  return (
    <div>
      {ranking.map((entry) => (
        <div key={entry.userId}>
          <span>{entry.momentaryPosition}. {entry.userName}</span>
          <span>{entry.totalPoints} + {entry.momentaryPoints}</span>
          {entry.positionChange > 0 && <span>↑ {entry.positionChange}</span>}
          {entry.isLeader && <span>👑</span>}
        </div>
      ))}
    </div>
  );
}
```

---

## 📦 Arquivos Criados/Modificados

### ✨ Arquivos Novos

1. ✅ `Application/Services/GroupStandingService.cs` - Serviço de classificação
2. ✅ `Application/DTOs/Match/TeamStandingDto.cs` - DTO para time na classificação
3. ✅ `Application/DTOs/Match/GroupStandingDto.cs` - DTO para grupo com classificação
4. ✅ `Application/DTOs/Ranking/RealTimeRankingEntryDto.cs` - DTO para ranking dinâmico
5. ✅ `FRONTEND_INTEGRATION_GUIDE.md` - Guia completo de integração
6. ✅ `CONSOLE_PROMPTS_FRONTEND.md` - Exemplos de console e testes
7. ✅ `IMPLEMENTATION_SUMMARY.md` - Este arquivo

### 🔧 Arquivos Modificados

1. ✅ `Controllers/GroupsController.cs` - Adicionados 2 novos endpoints
2. ✅ `Controllers/RankingController.cs` - Adicionado endpoint real-time
3. ✅ `Infrastructure/Hubs/RankingHub.cs` - Adicionados métodos de conexão
4. ✅ `Application/Services/RankingService.cs` - Import de entidades
5. ✅ `Application/Services/PredictionService.cs` - Notificações SignalR
6. ✅ `Application/Services/BolaoGroupService.cs` - Novo método real-time
7. ✅ `Program.cs` - Injeção de GroupStandingService

---

## 🧪 Como Testar

### 1. Via Swagger/Postman

```bash
# Obter classificação do Grupo A
GET https://localhost:5001/api/groups/A/standings
Authorization: Bearer {token}

# Obter classificações de todos os grupos
GET https://localhost:5001/api/groups/standings/all
Authorization: Bearer {token}

# Obter ranking em tempo real
GET https://localhost:5001/api/ranking/real-time/{groupId}
Authorization: Bearer {token}
```

### 2. Via Console/Terminal

```bash
# Usando curl
curl -H "Authorization: Bearer $TOKEN" \
  https://localhost:5001/api/groups/A/standings

# Usando PowerShell
$headers = @{ "Authorization" = "Bearer $token" }
Invoke-RestMethod -Uri "https://localhost:5001/api/groups/A/standings" `
  -Headers $headers
```

### 3. Via Frontend (Console)

Veja `CONSOLE_PROMPTS_FRONTEND.md` para exemplos completos de código JavaScript/TypeScript.

---

## 📈 Performance

### Cache Strategy
- **Classificações:** 30 segundos (invalida quando jogo termina)
- **Ranking em tempo real:** Sem cache (sempre atualizado)
- **Ranking detalhado:** 60 segundos

### Queries Otimizadas
- Usa `.Include()` para eager loading
- Filtra no banco de dados, não em memória
- Usa `Select()` para projetar apenas campos necessários

### Escalabilidade
- SignalR com grupos para isolamento de dados
- Cada grupo tem seu próprio hub group
- Notificações são enviadas apenas para interessados

---

## 🔐 Segurança

✅ Todos os endpoints requerem autenticação JWT
✅ Validação de membership para grupos
✅ Autorização baseada em claims
✅ Dados sanitizados nas respostas

---

## 📝 Próximos Passos (Opcional)

1. **Animações:** Adicionar transições suaves quando posição muda
2. **Notificações:** Push notification quando jogador sobe de posição
3. **Histórico:** Guardar snapshots do ranking a cada atualização
4. **Badges:** Troféus para milestones (1º lugar, 100 pontos, etc)
5. **Analytics:** Dashboard de acurácia e trending

---

## ✅ Checklist de Conclusão

- [x] Serviço de classificação dos grupos implementado
- [x] Ranking em tempo real com pontos momentâneos
- [x] SignalR hub com eventos de atualização
- [x] Endpoints REST documentados
- [x] DTOs criados e tipados
- [x] Injeção de dependência configurada
- [x] Compilação sem erros ✅
- [x] Documentação completa
- [x] Exemplos de frontend prontos
- [x] Guia de integração com console prompts

---

## 📞 Suporte

Para problemas ou dúvidas sobre a implementação, consulte:
1. `FRONTEND_INTEGRATION_GUIDE.md` - Documentação completa
2. `CONSOLE_PROMPTS_FRONTEND.md` - Exemplos de uso
3. Logs via Swagger/API Explorer

**Status:** ✅ **PRONTO PARA PRODUÇÃO**
