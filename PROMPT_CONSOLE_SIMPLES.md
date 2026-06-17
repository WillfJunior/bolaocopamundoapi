# 📡 Guia Simples - Endpoints para Frontend

## 🔑 Autenticação

Todo request precisa do header:
```
Authorization: Bearer {SEU_TOKEN_JWT}
```

---

## 1️⃣ CLASSIFICAÇÃO DA COPA

### 📊 Obter Classificação de Um Grupo

**Request:**
```
GET https://localhost:5001/api/groups/A/standings
Authorization: Bearer {token}
```

**Response (200):**
```json
{
  "groupName": "A",
  "teams": [
    {
      "teamId": 1,
      "teamName": "Brasil",
      "fifaCode": "BRA",
      "flagUrl": "https://...",
      "position": 1,
      "played": 3,
      "won": 3,
      "drawn": 0,
      "lost": 0,
      "goalsFor": 9,
      "goalsAgainst": 1,
      "goalDifference": 8,
      "points": 9
    },
    {
      "teamId": 2,
      "teamName": "México",
      "fifaCode": "MEX",
      "flagUrl": "https://...",
      "position": 2,
      "played": 3,
      "won": 2,
      "drawn": 1,
      "lost": 0,
      "goalsFor": 7,
      "goalsAgainst": 2,
      "goalDifference": 5,
      "points": 7
    }
  ],
  "matches": [
    {
      "id": 1,
      "homeTeam": {
        "id": 1,
        "name": "Brasil",
        "fifaCode": "BRA",
        "flagUrl": "https://..."
      },
      "awayTeam": {
        "id": 2,
        "name": "México",
        "fifaCode": "MEX",
        "flagUrl": "https://..."
      },
      "groupName": "A",
      "phase": 1,
      "status": 3,
      "matchDate": "2026-06-21T18:00:00",
      "homeScore": 3,
      "awayScore": 1,
      "venue": "Estádio",
      "matchLabel": "Jogo 1",
      "matchday": 1
    }
  ]
}
```

---

### 📊 Obter Todos os Grupos

**Request:**
```
GET https://localhost:5001/api/groups/standings/all
Authorization: Bearer {token}
```

**Response (200):**
```json
[
  {
    "groupName": "A",
    "teams": [...],
    "matches": [...]
  },
  {
    "groupName": "B",
    "teams": [...],
    "matches": [...]
  },
  {
    "groupName": "C",
    "teams": [...],
    "matches": [...]
  }
]
```

---

## 2️⃣ RANKING EM TEMPO REAL (PRINCIPAL)

### 🏆 Obter Ranking com Pontos Momentâneos

**Request:**
```
GET https://localhost:5001/api/ranking/real-time/{groupId}
Authorization: Bearer {token}
```

**Substituir:**
- `{groupId}` = ID do grupo do bolão (ex: 550e8400-e29b-41d4-a716-446655440000)

**Response (200):**
```json
[
  {
    "position": 1,
    "userId": "550e8400-e29b-41d4-a716-446655440001",
    "userName": "João Silva",
    "userPhotoUrl": "https://...",
    "totalPoints": 45,
    "exactScores": 8,
    "correctOutcomes": 12,
    "totalPredictions": 40,
    "errors": 20,
    "momentaryPoints": 3,
    "momentaryPosition": 1,
    "positionChange": 0,
    "isLeader": true,
    "pointsDifference": 0,
    "updatedAt": "2026-06-21T18:30:45"
  },
  {
    "position": 2,
    "userId": "550e8400-e29b-41d4-a716-446655440002",
    "userName": "Maria Santos",
    "userPhotoUrl": "https://...",
    "totalPoints": 42,
    "exactScores": 6,
    "correctOutcomes": 14,
    "totalPredictions": 40,
    "errors": 20,
    "momentaryPoints": 4,
    "momentaryPosition": 1,
    "positionChange": 1,
    "isLeader": false,
    "pointsDifference": 1,
    "updatedAt": "2026-06-21T18:30:45"
  }
]
```

**Campos Explicados:**
- `position` = Posição atual (antes dos pontos momentâneos)
- `totalPoints` = Pontos finalizados
- `momentaryPoints` = Pontos ganhando AGORA (enquanto jogo está rodando)
- `momentaryPosition` = Posição se parar AGORA
- `positionChange` = Movimento: `position - momentaryPosition` (positivo = subindo)
- `isLeader` = true se é 1º lugar momentâneamente
- `pointsDifference` = Diferença para o líder momentâneo
- `updatedAt` = Hora da última atualização

---

## 3️⃣ RANKING DETALHADO DO GRUPO

### 📈 Obter Ranking Completo com Métricas

**Request:**
```
GET https://localhost:5001/api/ranking?groupId={groupId}
Authorization: Bearer {token}
```

**Substituir:**
- `{groupId}` = ID do grupo do bolão

**Response (200):**
```json
{
  "groupId": "550e8400-e29b-41d4-a716-446655440000",
  "groupName": "Bolão Friends",
  "groupDescription": "Bolão entre amigos",
  "totalMembers": 5,
  "totalMatches": 80,
  "processedMatches": 12,
  "creatorName": "João Silva",
  "rankings": [
    {
      "position": 1,
      "userId": "550e8400-e29b-41d4-a716-446655440001",
      "userName": "João Silva",
      "userPhotoUrl": "https://...",
      "totalPoints": 45,
      "exactScores": 8,
      "correctOutcomes": 12,
      "totalPredictions": 40,
      "pointsPerPrediction": 1.13,
      "accuracyRate": 62.5,
      "isLeader": true,
      "pointsDifference": 0
    }
  ],
  "generatedAt": "2026-06-21T18:30:45"
}
```

---

## 4️⃣ WEBSOCKET (TEMPO REAL)

### 🔌 Conectar ao WebSocket

**URL:**
```
wss://localhost:5001/hubs/ranking
```

**Headers:**
```
Authorization: Bearer {token}
```

### Entrar em um Grupo

**Enviar:**
```
{ method: "JoinGroupRanking", args: ["550e8400-e29b-41d4-a716-446655440000"] }
```

### Sair de um Grupo

**Enviar:**
```
{ method: "LeaveGroupRanking", args: ["550e8400-e29b-41d4-a716-446655440000"] }
```

### Entrar no Ranking Global

**Enviar:**
```
{ method: "JoinGlobalRanking", args: [] }
```

### Sair do Ranking Global

**Enviar:**
```
{ method: "LeaveGlobalRanking", args: [] }
```

### 📨 Eventos Recebidos

#### Evento: Ranking do Grupo Atualizado

**Quando:** Um jogo do grupo foi finalizado e os resultados foram processados

**Dados:**
```json
{
  "type": "group-ranking-updated",
  "groupId": "550e8400-e29b-41d4-a716-446655440000",
  "matchId": 123
}
```

**Ação no Frontend:**
```
Fazer novo fetch de: GET /api/ranking/real-time/{groupId}
```

#### Evento: Ranking Global Atualizado

**Quando:** Qualquer jogo foi processado

**Dados:**
```json
{
  "type": "global-ranking-updated",
  "matchId": 123
}
```

**Ação no Frontend:**
```
Fazer novo fetch de: GET /api/ranking
```

#### Evento: Rankings Atualizados (Legado)

**Dados:**
```json
{
  "type": "rankings-updated",
  "matchId": 123
}
```

---

## 📋 Tabela Resumida

| O que fazer | Endpoint | Method | Descrição |
|------------|----------|--------|-----------|
| Classificação Grupo A | `/api/groups/A/standings` | GET | Um grupo específico |
| Todos Grupos | `/api/groups/standings/all` | GET | Todos os 8 grupos |
| Ranking Tempo Real | `/api/ranking/real-time/{id}` | GET | Com pontos momentâneos |
| Ranking Detalhado | `/api/ranking?groupId={id}` | GET | Com métricas completas |
| Conectar WebSocket | `/hubs/ranking` | WS | Eventos em tempo real |

---

## 🔄 Fluxo de Uso Prático

### Cenário 1: Exibir Classificação

```
1. GET /api/groups/A/standings
2. Receber list de times ordenados
3. Exibir tabela com posição, pontos, gols, etc
```

### Cenário 2: Exibir Ranking em Tempo Real

```
1. GET /api/ranking/real-time/{groupId}
2. Receber ranking com pontos momentâneos
3. Exibir:
   - Posição atual
   - Pontos + momentaryPoints
   - Se subindo/caindo (positionChange)
   - Se é líder (isLeader)
```

### Cenário 3: Atualizar em Tempo Real

```
1. Conectar ao WebSocket /hubs/ranking
2. Entrar no grupo: JoinGroupRanking(groupId)
3. Escutar evento "group-ranking-updated"
4. Quando evento chegar: Fazer fetch GET /api/ranking/real-time/{groupId}
5. Atualizar tabela na tela
```

---

## ❌ Erros Possíveis

### 401 - Não Autorizado
```
Problema: Token JWT não foi enviado ou é inválido
Solução: Verificar se Authorization header está presente e correto
```

### 403 - Forbidden
```
Problema: Usuário não é membro do grupo
Solução: Usuário deve entrar no grupo primeiro
```

### 404 - Não Encontrado
```
Problema: Grupo ou recurso não existe
Solução: Verificar se ID/nome está correto
```

### 500 - Erro do Servidor
```
Problema: Erro interno na API
Solução: Verificar logs da API
```

---

## 🧪 Exemplos via CURL

### Classificação do Grupo A
```bash
curl -X GET "https://localhost:5001/api/groups/A/standings" \
  -H "Authorization: Bearer $TOKEN"
```

### Todos os Grupos
```bash
curl -X GET "https://localhost:5001/api/groups/standings/all" \
  -H "Authorization: Bearer $TOKEN"
```

### Ranking em Tempo Real
```bash
curl -X GET "https://localhost:5001/api/ranking/real-time/550e8400-e29b-41d4-a716-446655440000" \
  -H "Authorization: Bearer $TOKEN"
```

### Ranking Detalhado
```bash
curl -X GET "https://localhost:5001/api/ranking?groupId=550e8400-e29b-41d4-a716-446655440000" \
  -H "Authorization: Bearer $TOKEN"
```

---

## 🎯 O QUE O FRONTEND PRECISA FAZER

### Mínimo (Funcional)
```
1. Buscar classificação: GET /api/groups/{name}/standings
2. Buscar ranking: GET /api/ranking/real-time/{groupId}
3. Exibir em tabelas/cards
4. Atualizar a cada 5-10 segundos (polling)
```

### Ideal (Tempo Real)
```
1. Conectar WebSocket ao /hubs/ranking
2. Entrar no grupo: JoinGroupRanking(groupId)
3. Escutar eventos de atualização
4. Quando evento chega: Refetch dos dados
5. Animar mudanças na tela (pontos, posição)
```

---

## 📚 Campos Disponíveis

### Para Classificação (Group)
```
- teamId (int)
- teamName (string)
- fifaCode (string)
- flagUrl (string)
- position (int)
- played (int)
- won (int)
- drawn (int)
- lost (int)
- goalsFor (int)
- goalsAgainst (int)
- goalDifference (int)
- points (int)
```

### Para Ranking Tempo Real
```
- position (int)
- userId (guid)
- userName (string)
- userPhotoUrl (string)
- totalPoints (int)
- exactScores (int)
- correctOutcomes (int)
- totalPredictions (int)
- errors (int)
- momentaryPoints (int) ⭐ PRINCIPAL
- momentaryPosition (int) ⭐ PRINCIPAL
- positionChange (int) ⭐ PRINCIPAL
- isLeader (bool) ⭐ PRINCIPAL
- pointsDifference (int)
- updatedAt (datetime)
```

---

## ✅ Checklist Integração

- [ ] Buscar classificação dos grupos
- [ ] Exibir tabela de classificação
- [ ] Buscar ranking em tempo real
- [ ] Exibir ranking com pontos momentâneos
- [ ] Conectar ao WebSocket
- [ ] Entrar no grupo no WebSocket
- [ ] Escutar eventos "group-ranking-updated"
- [ ] Refetch de dados quando evento chegar
- [ ] Animar mudança de posição (↑/↓)
- [ ] Destacar líder momentâneo
- [ ] Exibir pontos momentâneos em cores diferentes
- [ ] Tratamento de erros 401/403/404
- [ ] Funcionar em conexão lenta
- [ ] Teste com múltiplos grupos

---

## 🚀 Vamos Começar!

Use qualquer linguagem/framework:
- JavaScript/TypeScript
- React/Vue/Angular
- Mobile (React Native/Flutter)
- Qualquer coisa que fale HTTP + WebSocket

Os endpoints são agnósticos à tecnologia! 🎉
