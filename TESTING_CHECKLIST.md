# Checklist de Testes - Classificação em Tempo Real

## 🧪 Testes Unitários

### RankingService
- [ ] `CalculateMomentaryPoints` - Calcula corretamente pontos de um usuário
- [ ] `GetRankingAsync` - Retorna ranking ordenado por pontos, desempates
- [ ] `GetUserPositionAsync` - Encontra posição de um usuário específico
- [ ] Cache invalidation funciona quando jogo é processado

### GroupStandingService
- [ ] Classificação ordena por: Pontos → Saldo → Gols → Nome
- [ ] Pontos calculados corretamente (3V, 1E, 0D)
- [ ] Saldo de gols calculado corretamente
- [ ] Cache de 30s funciona
- [ ] Cache é invalidado quando jogo termina

### BolaoGroupService
- [ ] `GetRealTimeGroupRankingAsync` retorna todos os usuários
- [ ] Pontos momentâneos calculados corretamente
- [ ] Posição momentânea recalculada corretamente
- [ ] PositionChange calculado corretamente (position - momentaryPosition)
- [ ] IsLeader = true apenas para 1º colocado momentâneamente
- [ ] PointsDifference calculado corretamente

### PredictionService
- [ ] `ProcessMatchPredictionsAsync` calcula pontos corretamente
- [ ] Cache é invalidado para os grupos afetados
- [ ] SignalR envia eventos para os grupos corretos

---

## 🌐 Testes de Integração

### Endpoints REST

#### GET `/api/groups/{name}/standings`
- [ ] Retorna 200 com dados válidos para grupo existente
- [ ] Retorna 404 para grupo inexistente
- [ ] Requer autenticação (401 sem token)
- [ ] Times estão ordenados corretamente
- [ ] Jogos incluídos na resposta
- [ ] Cache funciona (resposta rápida em 2ª chamada)

**Test Case:**
```bash
GET /api/groups/A/standings
Authorization: Bearer {token}
Expect: 200, GroupStandingDto with 4 teams ordered correctly
```

#### GET `/api/groups/standings/all`
- [ ] Retorna lista de todos os grupos
- [ ] Cada grupo tem seus times
- [ ] Performance aceitável (< 500ms)

#### GET `/api/ranking/real-time/{groupId}`
- [ ] Retorna 200 com dados válidos
- [ ] Retorna 403 se usuário não é membro do grupo
- [ ] Retorna 404 se grupo não existe
- [ ] Todos os usuários incluídos
- [ ] MomentaryPoints >= 0
- [ ] MomentaryPosition entre 1 e total de membros
- [ ] PositionChange calculado corretamente
- [ ] IsLeader = true para primeiro da lista
- [ ] Sem cache (sempre atualizado)

**Test Case:**
```bash
GET /api/ranking/real-time/550e8400-e29b-41d4-a716-446655440000
Authorization: Bearer {token}
Expect: 200, List<RealTimeRankingEntryDto> sorted by momentaryPosition
```

#### GET `/api/ranking?groupId={groupId}`
- [ ] Retorna ranking detalhado
- [ ] Inclui métricas: PointsPerPrediction, AccuracyRate
- [ ] IsLeader = true apenas para 1º
- [ ] PointsDifference = 0 para 1º

---

### WebSocket (SignalR)

#### Conexão
- [ ] Hub está registrado em `/hubs/ranking`
- [ ] Requer autenticação JWT
- [ ] Conexão estabelecida com `withAutomaticReconnect()`
- [ ] Desconexão automática após timeout

**Test Case:**
```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl('/hubs/ranking', { accessTokenFactory: () => token })
  .build();

await connection.start();
// Expect: Connected ✓
```

#### JoinGroupRanking
- [ ] Usuário entra no grupo `group-ranking-{groupId}`
- [ ] Recebe eventos posteriores desse grupo
- [ ] Sem erro se já estava no grupo

**Test Case:**
```javascript
await connection.invoke('JoinGroupRanking', groupId);
// Expect: No error, group added
```

#### LeaveGroupRanking
- [ ] Usuário sai do grupo
- [ ] Não recebe mais eventos desse grupo

#### JoinGlobalRanking / LeaveGlobalRanking
- [ ] Mesma lógica dos métodos de grupo

#### Eventos Recebidos
- [ ] `group-ranking-updated` - dispara quando ranking do grupo muda
- [ ] `global-ranking-updated` - dispara quando ranking global muda
- [ ] `rankings-updated` - compatibilidade (legado)

**Test Case:**
```javascript
connection.on('group-ranking-updated', (gId, matchId) => {
  console.log(`Ranking do grupo ${gId} atualizado - Match ${matchId}`);
  // Fazer fetch de /api/ranking/real-time/{gId}
  // Expect: Dados atualizados
});

// Trigger: Admin atualiza resultado do jogo
// Expect: Evento recebido em < 1 segundo
```

---

## 🎬 Testes de Cenário (E2E)

### Cenário 1: Jogo em Andamento

**Setup:**
- Grupo "Bolão A" com 3 usuários: João, Maria, Pedro
- João predisse Brasil 2x1 Panamá
- Maria predisse Brasil 1x1 Panamá
- Pedro predisse Brasil 3x1 Panamá
- Resultado atual: Brasil 1x0 Panamá (45' em andamento)

**Teste:**
1. [ ] GET `/api/ranking/real-time/groupA` - Maria não tem pontos (1x1 errado)
2. [ ] GET `/api/ranking/real-time/groupA` - João tem 1 ponto (parcial, 2x1 esperado)
3. [ ] GET `/api/ranking/real-time/groupA` - Pedro tem 1 ponto (parcial, 3x1 esperado)
4. [ ] João continua em 1º com pontos momentâneos
5. [ ] Maria pode ficar em último (sem pontos momentâneos)

**Expected Result:**
```json
[
  { "userName": "João", "momentaryPoints": 1, "momentaryPosition": 1 },
  { "userName": "Pedro", "momentaryPoints": 1, "momentaryPosition": 2 },
  { "userName": "Maria", "momentaryPoints": 0, "momentaryPosition": 3 }
]
```

### Cenário 2: Gol nos Últimos Minutos

**Setup:** (continua de Cenário 1)
- Resultado atual: Brasil 2x1 Panamá (89')

**Teste:**
1. [ ] WebSocket dispara evento `group-ranking-updated`
2. [ ] Frontend recebe evento em < 1 segundo
3. [ ] GET `/api/ranking/real-time/groupA` retorna João com 3 pontos (acerto exato!)
4. [ ] João passa para 1º lugar (se não estava)
5. [ ] PositionChange exibe movimento correto

**Expected Result:**
```json
{ "userName": "João", "momentaryPoints": 3, "positionChange": 0 }
```

### Cenário 3: Jogo Finalizado

**Setup:** (continua de Cenário 2)
- Resultado final confirmado: Brasil 2x1 Panamá
- Admin marca jogo como `Finished`

**Teste:**
1. [ ] `PredictionService.ProcessMatchPredictionsAsync` é chamado
2. [ ] Pontos são salvos no banco (IsProcessed = true)
3. [ ] Cache de ranking é invalidado
4. [ ] GET `/api/ranking/real-time/groupA` retorna dados finalizados
5. [ ] MomentaryPoints permanece como Points finais

### Cenário 4: Múltiplos Grupos Simultâneos

**Setup:**
- 3 grupos diferentes: Friends, Colleagues, Family
- Jogos acontecendo em todos os grupos

**Teste:**
1. [ ] Conexão SignalR recebe eventos de todos os grupos
2. [ ] Performance não degrada
3. [ ] Cache não é compartilhado entre grupos
4. [ ] Isolamento de dados (usuário do Grupo A não vê dados do Grupo B)

**Expected Result:**
- Latência < 100ms entre evento e recebimento
- CPU < 20% com 3 conexões ativas

---

## 📊 Testes de Performance

### Carga de Dados

- [ ] 100 usuários em 1 grupo
- [ ] 10 jogos simultâneos
- [ ] 50 conexões WebSocket ativas

**Teste:**
```
GET /api/ranking/real-time/{groupId} com 100 usuários
Expect: Resposta em < 200ms
Expect: Memória usada < 100MB
```

### Cache

- [ ] Primeira requisição: ~50ms (sem cache)
- [ ] Segunda requisição (dentro de 30s): ~5ms (com cache)
- [ ] Terceira requisição (após 31s): ~50ms (cache expirado)

### WebSocket

- [ ] 50 conexões simultâneas
- [ ] Broadcast de evento para 50 clientes: < 500ms
- [ ] Memória por conexão: < 1MB

---

## 🔐 Testes de Segurança

### Autenticação

- [ ] Endpoint sem token retorna 401
- [ ] Token inválido retorna 401
- [ ] Token expirado retorna 401
- [ ] WebSocket sem token falha ao conectar

### Autorização

- [ ] Usuário não pode acessar ranking de grupo que não é membro
- [ ] Usuário não pode ver dados de outros grupos
- [ ] Admin não pode processar com privilégios de usuário normal

**Test Case:**
```bash
# Usuário não é membro do grupo
GET /api/ranking/real-time/550e8400-e29b-41d4-a716-446655440001
Authorization: Bearer {token}
Expect: 403 Forbidden
```

### Validação de Dados

- [ ] GroupId inválido retorna erro
- [ ] MatchId inválido retorna erro
- [ ] Scores negativos são rejeitados
- [ ] Dados não sanitizados são tratados

---

## 🐛 Testes de Edge Cases

### Casos Extremos

- [ ] Jogo com score 10x10
- [ ] Grupo com 1 membro
- [ ] Usuário com 0 palpites
- [ ] Jogo em progresso há 10 minutos
- [ ] Jogo com 0 segundos de duração

### Condições de Erro

- [ ] Timeout na conexão com BD durante ranking
- [ ] Cache corrompido
- [ ] SignalR desconectado
- [ ] Múltiplos updates simultâneos do mesmo jogo

---

## ✅ Testes de Regressão

### Compatibilidade

- [ ] Endpoints antigos continuam funcionando
- [ ] Evento `rankings-updated` (legado) ainda é enviado
- [ ] DTOs antigos não foram modificados
- [ ] RankingService continua funcionando

### Funcionalidade Existente

- [ ] GET `/api/ranking` (global) ainda funciona
- [ ] GET `/api/ranking/me` ainda funciona
- [ ] GET `/api/ranking/by-group` ainda funciona
- [ ] Controllers antigos não foram quebrados

---

## 📋 Plano de Execução

### Phase 1: Unitários (1h)
```
1. Build solution ✓
2. Run unit tests
3. Verify all services
```

### Phase 2: Integração (2h)
```
1. Start API server
2. Test all endpoints via Postman
3. Test WebSocket connections
4. Verify database queries
```

### Phase 3: Cenários (1h)
```
1. Setup test data
2. Run E2E scenarios
3. Verify event flow
4. Check performance
```

### Phase 4: Produção (30min)
```
1. Load test
2. Security review
3. Final verification
4. Deploy
```

---

## 🔍 Debugging Commands

### PowerShell

```powershell
# Build e run
dotnet build
dotnet run --project BolaoCopaMundo

# Check running processes
Get-Process | Where-Object { $_.Name -like "*dotnet*" }

# View logs
Get-Content ./logs/app.log -Tail 100

# API Health Check
$token = "seu_token_jwt"
$headers = @{ "Authorization" = "Bearer $token" }
Invoke-RestMethod -Uri "https://localhost:5001/api/groups/A/standings" `
  -Headers $headers | ConvertTo-Json
```

### JavaScript Console

```javascript
// Teste de ranking em tempo real
const token = localStorage.getItem('token');
const groupId = 'seu_group_id';

const response = await fetch(
  `https://localhost:5001/api/ranking/real-time/${groupId}`,
  { headers: { 'Authorization': `Bearer ${token}` } }
);
const data = await response.json();
console.table(data.map(e => ({
  Nome: e.userName,
  Pontos: e.totalPoints,
  Momentâneo: e.momentaryPoints,
  Posição: e.momentaryPosition,
  Movimento: e.positionChange
})));
```

---

## 📊 Métricas de Sucesso

✅ **Todos os testes passam**
✅ **Performance < 200ms para requests**
✅ **Sem memory leaks após 1h**
✅ **100% de cobertura de cenários críticos**
✅ **Código compila sem warnings**
✅ **Documentação completa**
✅ **Pronto para produção**
