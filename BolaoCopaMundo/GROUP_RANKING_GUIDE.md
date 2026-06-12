# Guia de Ranking por Grupo - Integração Frontend

## 📊 Novo Endpoint: Ranking Detalhado do Grupo

### GET /api/bolao-groups/{groupId}/ranking/detailed

Retorna um ranking **completo e detalhado** do grupo com análises, estatísticas e informações contextuais.

---

## 🔌 Requisição

```bash
curl -X GET \
  "https://api.seu-servidor.com/api/bolao-groups/{groupId}/ranking/detailed" \
  -H "Authorization: Bearer {token}"
```

### Parâmetros

| Parâmetro | Tipo | Obrigatório | Descrição |
|-----------|------|-------------|-----------|
| groupId | UUID | ✅ | ID do grupo (URL path) |
| token | JWT | ✅ | Token de autenticação |

**Nota:** Você deve ser membro ativo do grupo para acessar este endpoint.

---

## 📋 Resposta

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
      "userPhotoUrl": "https://api.seu-servidor.com/photos/maria.jpg",
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
      "userPhotoUrl": "https://api.seu-servidor.com/photos/pedro.jpg",
      "totalPoints": 38,
      "exactScores": 7,
      "correctOutcomes": 10,
      "totalPredictions": 30,
      "pointsPerPrediction": 1.27,
      "accuracyRate": 70.0,
      "isLeader": false,
      "pointsDifference": 4
    },
    {
      "position": 3,
      "userId": "550e8400-e29b-41d4-a716-446655440003",
      "userName": "Ana Silva",
      "userPhotoUrl": "https://api.seu-servidor.com/photos/ana.jpg",
      "totalPoints": 35,
      "exactScores": 6,
      "correctOutcomes": 9,
      "totalPredictions": 28,
      "pointsPerPrediction": 1.25,
      "accuracyRate": 69.0,
      "isLeader": false,
      "pointsDifference": 7
    }
  ]
}
```

---

## 📊 Campos de Resposta

### Nível de Grupo

| Campo | Tipo | Descrição |
|-------|------|-----------|
| groupId | UUID | ID único do grupo |
| groupName | string | Nome do grupo |
| groupDescription | string? | Descrição do grupo |
| totalMembers | int | Total de membros ativos |
| totalMatches | int | Total de jogos da competição |
| processedMatches | int | Jogos com resultados processados |
| creatorName | string | Nome do criador do grupo |
| generatedAt | ISO8601 | Timestamp da geração |

### Nível de Entrada no Ranking

| Campo | Tipo | Descrição |
|-------|------|-----------|
| position | int | Posição no ranking (1º lugar, 2º lugar, etc) |
| userId | UUID | ID do usuário |
| userName | string | Nome do usuário |
| userPhotoUrl | string? | URL da foto do perfil |
| **totalPoints** | int | **Total de pontos ganhos** |
| exactScores | int | Quantidade de acertos de placar exato |
| correctOutcomes | int | Quantidade de acertos de resultado |
| totalPredictions | int | Total de palpites feitos |
| **pointsPerPrediction** | float | **Média de pontos por palpite** (0.00 a 1.50) |
| **accuracyRate** | float | **Taxa de acurácia em %** (0% a 100%) |
| isLeader | bool | `true` se está em 1º lugar |
| pointsDifference | int | Diferença de pontos em relação ao líder |

---

## 🎯 Casos de Uso

### 1. Exibir Leaderboard Completo

```typescript
// Vue 3
const { data: groupRanking } = await useFetch('/api/bolao-groups/{groupId}/ranking/detailed', {
  headers: { 'Authorization': `Bearer ${token}` }
});

// Renderizar tabela
<table>
  <tr v-for="entry in groupRanking.rankings" :key="entry.userId">
    <td>{{ entry.position }}</td>
    <td>{{ entry.isLeader ? '👑' : '' }} {{ entry.userName }}</td>
    <td>{{ entry.totalPoints }} pts</td>
    <td>{{ entry.accuracyRate }}%</td>
    <td v-if="!entry.isLeader">-{{ entry.pointsDifference }}</td>
  </tr>
</table>
```

### 2. Destacar Líder

```vue
<div v-if="groupRanking.rankings[0]" class="leader-card">
  <h2>🏆 Líder: {{ groupRanking.rankings[0].userName }}</h2>
  <p>{{ groupRanking.rankings[0].totalPoints }} pontos</p>
  <p>Acurácia: {{ groupRanking.rankings[0].accuracyRate }}%</p>
  <img :src="groupRanking.rankings[0].userPhotoUrl" />
</div>
```

### 3. Mostrar Progresso do Grupo

```typescript
const progressPercentage = 
  (groupRanking.processedMatches / groupRanking.totalMatches) * 100;

console.log(`${groupRanking.processedMatches}/${groupRanking.totalMatches} jogos processados`);
console.log(`${progressPercentage.toFixed(1)}% da competição concluída`);
```

### 4. Filtrar Apenas Membros com Palpites

```typescript
const activePredictors = groupRanking.rankings.filter(
  entry => entry.totalPredictions > 0
);
```

---

## 🔄 Integração com SignalR

Combine o ranking detalhado com WebSocket para atualizações em tempo real:

```typescript
// Conectar ao hub
const hubConnection = new signalR.HubConnectionBuilder()
  .withUrl("/hubs/ranking")
  .build();

await hubConnection.start();

// Ao receber evento, buscar ranking atualizado
hubConnection.on("rankings-updated", async (matchId) => {
  const response = await fetch(
    `/api/bolao-groups/${groupId}/ranking/detailed`,
    { headers: { 'Authorization': `Bearer ${token}` } }
  );
  groupRanking = await response.json();
  // Renderizar nova tabela
});
```

---

## 📱 Exemplo React

```typescript
import { useEffect, useState } from 'react';

export function GroupRankingPage({ groupId, token }) {
  const [ranking, setRanking] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchRanking = async () => {
      const res = await fetch(
        `/api/bolao-groups/${groupId}/ranking/detailed`,
        { headers: { 'Authorization': `Bearer ${token}` } }
      );
      setRanking(await res.json());
      setLoading(false);
    };

    fetchRanking();

    // Atualizar a cada 60s (complementar ao SignalR)
    const interval = setInterval(fetchRanking, 60000);
    return () => clearInterval(interval);
  }, [groupId, token]);

  if (loading) return <div>Carregando ranking...</div>;

  return (
    <div>
      <h1>🏆 {ranking.groupName}</h1>
      
      <div className="stats">
        <p>Membros: {ranking.totalMembers}</p>
        <p>Progresso: {ranking.processedMatches}/{ranking.totalMatches} jogos</p>
      </div>

      <table>
        <thead>
          <tr>
            <th>Posição</th>
            <th>Jogador</th>
            <th>Pontos</th>
            <th>Acurácia</th>
            <th>Diferença</th>
          </tr>
        </thead>
        <tbody>
          {ranking.rankings.map(entry => (
            <tr key={entry.userId} className={entry.isLeader ? 'leader' : ''}>
              <td>{entry.isLeader ? '👑' : ''} {entry.position}</td>
              <td>
                <img src={entry.userPhotoUrl} alt={entry.userName} />
                {entry.userName}
              </td>
              <td><strong>{entry.totalPoints}</strong></td>
              <td>{entry.accuracyRate}%</td>
              <td>{entry.isLeader ? '-' : `-${entry.pointsDifference}`}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
```

---

## 📊 Exemplo Vue 3 Completo

```vue
<template>
  <div class="group-ranking-container">
    <!-- Header -->
    <div class="header">
      <h1>{{ ranking?.groupName }} 🏆</h1>
      <p v-if="ranking?.groupDescription">{{ ranking.groupDescription }}</p>
      <p class="progress">
        Progresso: {{ ranking?.processedMatches }}/{{ ranking?.totalMatches }} jogos
        <progress 
          :value="ranking?.processedMatches" 
          :max="ranking?.totalMatches">
        </progress>
      </p>
    </div>

    <!-- Líder Destaque -->
    <div v-if="ranking?.rankings.length" class="leader-spotlight">
      <div class="leader-card">
        <img :src="ranking.rankings[0].userPhotoUrl" :alt="ranking.rankings[0].userName" />
        <h2>{{ ranking.rankings[0].userName }}</h2>
        <p class="crown">👑 Líder do Grupo</p>
        <p class="points">{{ ranking.rankings[0].totalPoints }} pontos</p>
        <p class="stats">
          <span>{{ ranking.rankings[0].exactScores }} acertos exatos</span>
          <span>{{ ranking.rankings[0].correctOutcomes }} acertos de resultado</span>
        </p>
        <p class="accuracy">Acurácia: {{ ranking.rankings[0].accuracyRate }}%</p>
      </div>
    </div>

    <!-- Tabela de Ranking -->
    <div class="ranking-table">
      <table>
        <thead>
          <tr>
            <th>Posição</th>
            <th>Jogador</th>
            <th>Pontos</th>
            <th>Acurácia</th>
            <th>Médias</th>
            <th>Diferença</th>
          </tr>
        </thead>
        <tbody>
          <tr 
            v-for="entry in ranking?.rankings" 
            :key="entry.userId"
            :class="{ leader: entry.isLeader }"
          >
            <td class="position">
              <span v-if="entry.position === 1" class="medal">🥇</span>
              <span v-else-if="entry.position === 2" class="medal">🥈</span>
              <span v-else-if="entry.position === 3" class="medal">🥉</span>
              <span v-else>{{ entry.position }}</span>
            </td>
            
            <td class="player">
              <img 
                :src="entry.userPhotoUrl" 
                :alt="entry.userName"
                class="avatar"
              />
              {{ entry.userName }}
            </td>
            
            <td class="points"><strong>{{ entry.totalPoints }}</strong></td>
            
            <td class="accuracy">
              <div class="accuracy-bar">
                <div 
                  class="accuracy-fill"
                  :style="{ width: `${entry.accuracyRate}%` }"
                ></div>
              </div>
              {{ entry.accuracyRate }}%
            </td>
            
            <td class="metrics">
              {{ entry.pointsPerPrediction.toFixed(2) }} pts/palpite
            </td>
            
            <td class="difference">
              <span v-if="entry.isLeader" class="leader-badge">LÍDER</span>
              <span v-else class="gap">-{{ entry.pointsDifference }}</span>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Footer com Metadados -->
    <div class="footer">
      <p>Criado por: {{ ranking?.creatorName }}</p>
      <p>Membros: {{ ranking?.totalMembers }}</p>
      <p class="updated">Atualizado: {{ formatDate(ranking?.generatedAt) }}</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import type { GroupRankingResponseDto } from '@/api/types';

const props = defineProps<{
  groupId: string;
  token: string;
}>();

const ranking = ref<GroupRankingResponseDto | null>(null);

const formatDate = (date: string) => {
  return new Date(date).toLocaleString('pt-BR');
};

onMounted(async () => {
  const response = await fetch(
    `/api/bolao-groups/${props.groupId}/ranking/detailed`,
    { headers: { 'Authorization': `Bearer ${props.token}` } }
  );
  ranking.value = await response.json();
});
</script>

<style scoped>
.group-ranking-container {
  max-width: 1000px;
  margin: 0 auto;
  padding: 20px;
}

.header {
  text-align: center;
  margin-bottom: 40px;
}

.header h1 {
  font-size: 2.5em;
  margin: 0;
}

.progress {
  margin-top: 10px;
}

progress {
  width: 100%;
  margin-top: 8px;
  height: 8px;
}

.leader-spotlight {
  margin-bottom: 40px;
  text-align: center;
}

.leader-card {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 30px;
  border-radius: 12px;
  box-shadow: 0 10px 30px rgba(0,0,0,0.2);
}

.leader-card img {
  width: 100px;
  height: 100px;
  border-radius: 50%;
  border: 4px solid white;
  margin-bottom: 15px;
}

.leader-card h2 {
  margin: 15px 0;
  font-size: 1.8em;
}

.crown {
  font-size: 1.2em;
  margin: 10px 0;
}

.points {
  font-size: 2em;
  font-weight: bold;
  margin: 10px 0;
}

.stats {
  display: flex;
  justify-content: center;
  gap: 20px;
  margin: 15px 0;
  font-size: 0.9em;
}

.accuracy {
  font-size: 1.1em;
}

/* Tabela */
.ranking-table {
  overflow-x: auto;
  margin-bottom: 30px;
}

table {
  width: 100%;
  border-collapse: collapse;
  background: white;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
  border-radius: 8px;
  overflow: hidden;
}

thead {
  background: #f5f5f5;
  font-weight: bold;
}

th {
  padding: 15px;
  text-align: left;
  border-bottom: 2px solid #ddd;
}

tbody tr {
  border-bottom: 1px solid #eee;
  transition: background 0.2s;
}

tbody tr:hover {
  background: #f9f9f9;
}

tbody tr.leader {
  background: #fff8f0;
  font-weight: bold;
}

td {
  padding: 15px;
}

.position {
  text-align: center;
  font-size: 1.2em;
}

.medal {
  font-size: 1.5em;
}

.player {
  display: flex;
  align-items: center;
  gap: 10px;
}

.avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
}

.points {
  text-align: center;
  font-size: 1.2em;
}

.accuracy {
  position: relative;
}

.accuracy-bar {
  background: #eee;
  height: 20px;
  border-radius: 10px;
  overflow: hidden;
  margin-bottom: 5px;
}

.accuracy-fill {
  background: linear-gradient(90deg, #4caf50, #8bc34a);
  height: 100%;
  transition: width 0.3s;
}

.metrics {
  text-align: center;
  font-size: 0.9em;
}

.difference {
  text-align: center;
}

.leader-badge {
  background: gold;
  color: #333;
  padding: 4px 8px;
  border-radius: 4px;
  font-weight: bold;
  font-size: 0.85em;
}

.gap {
  color: #999;
}

.footer {
  text-align: center;
  padding-top: 20px;
  border-top: 1px solid #eee;
  color: #666;
  font-size: 0.9em;
}

.updated {
  margin-top: 10px;
  font-size: 0.85em;
  color: #999;
}
</style>
```

---

## ⚡ Performance

- **Cache**: 60s (mesmo cache do endpoint simples)
- **Tempo de resposta**: ~50-100ms com cache quente
- **Índices otimizados**: `(GroupId, IsProcessed)`, `(MatchId, IsProcessed)`

---

## 🔐 Autenticação & Autorização

- ✅ Requer JWT token válido
- ✅ Requer ser membro ativo do grupo
- ✅ Retorna 401 se token inválido
- ✅ Retorna 403 se não é membro

---

## 📌 Resumo dos Endpoints

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/ranking` | Ranking global (simples, cacheado) |
| GET | `/api/ranking/me` | Sua posição no ranking global |
| GET | `/api/bolao-groups/{id}/ranking` | Ranking do grupo (simples, cacheado) |
| **GET** | **`/api/bolao-groups/{id}/ranking/detailed`** | **Ranking do grupo (detalhado, com análises)** |

---

## 🎯 Checklist Frontend

- [ ] Buscar endpoint `/api/bolao-groups/{groupId}/ranking/detailed`
- [ ] Renderizar card do líder com destaque
- [ ] Criar tabela com ranking completo
- [ ] Exibir barra de progresso da competição
- [ ] Mostrar acurácia com barra visual
- [ ] Indicar diferença de pontos para o líder
- [ ] Integrar com SignalR para atualizações automáticas
- [ ] Testar com múltiplos grupos
- [ ] Validar formatação de números (pontos, percentuais)
- [ ] Adicionar loading states

