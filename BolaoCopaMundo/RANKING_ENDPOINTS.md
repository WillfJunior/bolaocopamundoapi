# Documentação Completa - Endpoints de Ranking

## 🎯 Visão Geral

O sistema de ranking agora oferece três formas de obter dados:

1. **Ranking Global** — ranking de todos os usuários
2. **Ranking por Grupo** — separado por cada grupo do usuário
3. **Ranking Detalhado** — com análises estatísticas

---

## 📡 Endpoints

### 1. GET /api/ranking
**Ranking Global (Simples)**

Retorna o ranking de todos os usuários ativos da plataforma.

#### Requisição
```bash
curl -X GET \
  "http://localhost:5196/api/ranking" \
  -H "Authorization: Bearer {token}"
```

#### Resposta
```json
[
  {
    "position": 1,
    "userId": "550e8400-e29b-41d4-a716-446655440001",
    "userName": "Maria Santos",
    "userPhotoUrl": "https://...",
    "totalPoints": 42,
    "exactScores": 8,
    "correctOutcomes": 12,
    "totalPredictions": 30
  },
  {
    "position": 2,
    "userId": "550e8400-e29b-41d4-a716-446655440002",
    "userName": "Pedro Costa",
    "userPhotoUrl": "https://...",
    "totalPoints": 38,
    "exactScores": 7,
    "correctOutcomes": 10,
    "totalPredictions": 30
  }
]
```

---

### 2. GET /api/ranking/by-group
**Ranking Separado por Grupo (NOVO)**

Retorna o ranking de cada grupo em que o usuário é membro, com informações do grupo.

#### Requisição
```bash
curl -X GET \
  "http://localhost:5196/api/ranking/by-group" \
  -H "Authorization: Bearer {token}"
```

#### Resposta
```json
[
  {
    "groupId": "550e8400-e29b-41d4-a716-446655440000",
    "groupName": "Bolão dos Amigos",
    "groupDescription": "Bolão do grupo de amigos para Copa 2026",
    "totalMembers": 8,
    "rankings": [
      {
        "position": 1,
        "userId": "550e8400-e29b-41d4-a716-446655440001",
        "userName": "Maria Santos",
        "userPhotoUrl": "https://...",
        "totalPoints": 42,
        "exactScores": 8,
        "correctOutcomes": 12,
        "totalPredictions": 30
      },
      {
        "position": 2,
        "userId": "550e8400-e29b-41d4-a716-446655440002",
        "userName": "Pedro Costa",
        "userPhotoUrl": "https://...",
        "totalPoints": 38,
        "exactScores": 7,
        "correctOutcomes": 10,
        "totalPredictions": 30
      }
    ]
  },
  {
    "groupId": "550e8400-e29b-41d4-a716-446655440100",
    "groupName": "Bolão da Família",
    "groupDescription": "Bolão familiar",
    "totalMembers": 5,
    "rankings": [
      {
        "position": 1,
        "userId": "550e8400-e29b-41d4-a716-446655440003",
        "userName": "Ana Silva",
        "userPhotoUrl": "https://...",
        "totalPoints": 35,
        "exactScores": 6,
        "correctOutcomes": 9,
        "totalPredictions": 28
      }
    ]
  }
]
```

---

### 3. GET /api/ranking/me
**Sua Posição no Ranking Global**

Retorna apenas a posição e dados do usuário autenticado no ranking global.

#### Requisição
```bash
curl -X GET \
  "http://localhost:5196/api/ranking/me" \
  -H "Authorization: Bearer {token}"
```

#### Resposta
```json
{
  "position": 3,
  "userId": "550e8400-e29b-41d4-a716-446655440003",
  "userName": "Ana Silva",
  "userPhotoUrl": "https://...",
  "totalPoints": 35,
  "exactScores": 6,
  "correctOutcomes": 9,
  "totalPredictions": 28
}
```

---

### 4. GET /api/ranking?groupId={groupId}
**Ranking Detalhado de Um Grupo Específico**

Retorna ranking completo com análises e estatísticas de um grupo específico.

#### Requisição
```bash
curl -X GET \
  "http://localhost:5196/api/ranking?groupId=550e8400-e29b-41d4-a716-446655440000" \
  -H "Authorization: Bearer {token}"
```

#### Resposta
```json
{
  "groupId": "550e8400-e29b-41d4-a716-446655440000",
  "groupName": "Bolão dos Amigos",
  "groupDescription": "Bolão do grupo de amigos para Copa 2026",
  "totalMembers": 8,
  "totalMatches": 64,
  "processedMatches": 12,
  "creatorName": "João Silva",
  "generatedAt": "2026-06-12T02:15:00Z",
  "rankings": [
    {
      "position": 1,
      "userId": "550e8400-e29b-41d4-a716-446655440001",
      "userName": "Maria Santos",
      "userPhotoUrl": "https://...",
      "totalPoints": 42,
      "exactScores": 8,
      "correctOutcomes": 12,
      "totalPredictions": 30,
      "pointsPerPrediction": 1.4,
      "accuracyRate": 77.8,
      "isLeader": true,
      "pointsDifference": 0
    },
    {
      "position": 2,
      "userId": "550e8400-e29b-41d4-a716-446655440002",
      "userName": "Pedro Costa",
      "userPhotoUrl": "https://...",
      "totalPoints": 38,
      "exactScores": 7,
      "correctOutcomes": 10,
      "totalPredictions": 30,
      "pointsPerPrediction": 1.27,
      "accuracyRate": 70.0,
      "isLeader": false,
      "pointsDifference": 4
    }
  ]
}
```

---

### 5. GET /api/bolao-groups/{groupId}/ranking
**Ranking Simples do Grupo**

Retorna apenas o ranking básico de um grupo específico (sem análises).

#### Requisição
```bash
curl -X GET \
  "http://localhost:5196/api/bolao-groups/{groupId}/ranking" \
  -H "Authorization: Bearer {token}"
```

#### Resposta
```json
[
  {
    "position": 1,
    "userId": "550e8400-e29b-41d4-a716-446655440001",
    "userName": "Maria Santos",
    "userPhotoUrl": "https://...",
    "totalPoints": 42,
    "exactScores": 8,
    "correctOutcomes": 12,
    "totalPredictions": 30
  }
]
```

---

### 6. GET /api/bolao-groups/{groupId}/ranking/detailed
**Ranking Detalhado do Grupo**

Retorna ranking com análises estatísticas completas para um grupo.

#### Requisição
```bash
curl -X GET \
  "http://localhost:5196/api/bolao-groups/{groupId}/ranking/detailed" \
  -H "Authorization: Bearer {token}"
```

#### Resposta
Veja a seção **GROUP_RANKING_GUIDE.md** para detalhes completos.

---

## 📊 Comparação de Endpoints

| Endpoint | Dados Retornados | Use Case | Cache |
|----------|-----------------|----------|-------|
| `GET /api/ranking` | Ranking global simples | Dashboard global, tabela de líderes | ✅ 60s |
| `GET /api/ranking/by-group` | **Ranking separado por grupo** | **Página inicial com múltiplos grupos** | ❌ |
| `GET /api/ranking/me` | Sua posição global | Widget de posição pessoal | ✅ 60s |
| `GET /api/ranking?groupId={id}` | Ranking detalhado com análises | Página de detalhes do grupo | ✅ 60s |
| `GET /api/bolao-groups/{id}/ranking` | Ranking simples do grupo | Tabela rápida do grupo | ✅ 60s |
| `GET /api/bolao-groups/{id}/ranking/detailed` | Ranking com análises completas | Página premium de leaderboard | ✅ 60s |

---

## 🎯 Exemplo de Uso: Página Inicial com Múltiplos Grupos

### React
```typescript
import { useEffect, useState } from 'react';

export function HomePage() {
  const [groupRankings, setGroupRankings] = useState([]);
  const token = localStorage.getItem('authToken');

  useEffect(() => {
    // Buscar ranking de todos os grupos do usuário
    fetch('/api/ranking/by-group', {
      headers: { 'Authorization': `Bearer ${token}` }
    })
      .then(res => res.json())
      .then(setGroupRankings);
  }, [token]);

  return (
    <div className="groups-container">
      {groupRankings.map(group => (
        <div key={group.groupId} className="group-card">
          <h2>{group.groupName}</h2>
          <p>{group.totalMembers} membros</p>
          
          <table>
            <tbody>
              {group.rankings.slice(0, 5).map(entry => (
                <tr key={entry.userId}>
                  <td>#{entry.position}</td>
                  <td>{entry.userName}</td>
                  <td>{entry.totalPoints} pts</td>
                </tr>
              ))}
            </tbody>
          </table>

          <a href={`/group/${group.groupId}`}>Ver ranking completo →</a>
        </div>
      ))}
    </div>
  );
}
```

### Vue 3
```vue
<template>
  <div class="groups-container">
    <div v-for="group in groupRankings" :key="group.groupId" class="group-card">
      <h2>{{ group.groupName }}</h2>
      <p>{{ group.totalMembers }} membros</p>
      
      <table>
        <tr v-for="entry in group.rankings.slice(0, 5)" :key="entry.userId">
          <td>#{{ entry.position }}</td>
          <td>{{ entry.userName }}</td>
          <td>{{ entry.totalPoints }} pts</td>
        </tr>
      </table>

      <router-link :to="`/group/${group.groupId}`">
        Ver ranking completo →
      </router-link>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';

const groupRankings = ref([]);
const token = localStorage.getItem('authToken');

onMounted(async () => {
  const response = await fetch('/api/ranking/by-group', {
    headers: { 'Authorization': `Bearer ${token}` }
  });
  groupRankings.value = await response.json();
});
</script>
```

---

## 🔄 Integração com SignalR

Quando um ranking é atualizado (após processamento de pontos), o evento é disparado:

```javascript
hubConnection.on("rankings-updated", async (matchId) => {
  // Atualizar ranking global
  const globalRanking = await fetch('/api/ranking', {
    headers: { 'Authorization': `Bearer ${token}` }
  }).then(r => r.json());

  // Atualizar rankings por grupo
  const groupRankings = await fetch('/api/ranking/by-group', {
    headers: { 'Authorization': `Bearer ${token}` }
  }).then(r => r.json());

  // Renderizar novos dados
  updateUI(groupRankings);
});
```

---

## ⚡ Performance & Cache

- **Global Ranking**: Cacheado por 60s
- **By Group**: Sem cache (busca grupos do usuário em tempo real)
- **Detailed Group**: Cacheado por 60s (reusa cache de grupo)
- **My Position**: Calculado a partir do ranking global cacheado

**Índices otimizados:**
- `(GroupId, IsProcessed)` — otimiza queries de grupo
- `(MatchId, IsProcessed)` — otimiza processamento

---

## 🔐 Autenticação

Todos os endpoints requerem:
- ✅ JWT token válido via header `Authorization: Bearer {token}`
- ✅ Estar autenticado
- ✅ Para `/api/bolao-groups/{id}/ranking*` — ser membro ativo do grupo

---

## 📋 Campos Explicados

| Campo | Tipo | Descrição |
|-------|------|-----------|
| **position** | int | Posição no ranking (1º, 2º, etc) |
| **userId** | UUID | ID do usuário |
| **userName** | string | Nome do usuário |
| **userPhotoUrl** | string? | URL da foto de perfil |
| **totalPoints** | int | Total de pontos ganhos |
| **exactScores** | int | Quantidade de acertos de placar exato (3 pts cada) |
| **correctOutcomes** | int | Quantidade de acertos de resultado (1 pt cada) |
| **totalPredictions** | int | Total de palpites feitos |
| **pointsPerPrediction** | float | Média de pontos por palpite (0.00-1.50) |
| **accuracyRate** | float | Taxa de acurácia em % (0-100) |
| **isLeader** | bool | `true` se é o 1º lugar |
| **pointsDifference** | int | Diferença de pontos em relação ao líder |

---

## 🧪 Checklist Frontend

- [ ] Implementar GET `/api/ranking` para tabela global
- [ ] Implementar GET `/api/ranking/by-group` para página inicial
- [ ] Implementar GET `/api/ranking/me` para widget pessoal
- [ ] Implementar GET `/api/ranking?groupId={id}` para página de grupo detalhada
- [ ] Integrar com SignalR para atualizações automáticas
- [ ] Renderizar medalhas (🥇 🥈 🥉) para top 3
- [ ] Exibir barra de progresso de acurácia
- [ ] Mostrar diferença para o líder
- [ ] Adicionar loading states
- [ ] Testar com múltiplos grupos

---

## 🆘 Troubleshooting

| Problema | Solução |
|----------|---------|
| 401 - Não autorizado | Token expirado ou inválido |
| 403 - Acesso negado | Não é membro ativo do grupo |
| 404 - Grupo não encontrado | GroupId inválido |
| Rankings vazios | Sem predições processadas no grupo |
| Cache não atualiza | Aguarde 60s ou force refresh (F5) |

