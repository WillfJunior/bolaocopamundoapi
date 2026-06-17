# Guia de Integração Frontend - Classificação em Tempo Real da Copa e Ranking Dinâmico

## Visão Geral

O sistema foi implementado para fornecer:
1. **Classificação dos grupos da Copa** com critérios de desempate corretos
2. **Ranking em tempo real** do bolão com pontos momentâneos
3. **Atualizações via WebSocket (SignalR)** para experiência dinâmica

## Endpoints REST

### 1. Classificação dos Grupos da Copa

#### GET `/api/groups/{name}/standings`
Retorna a classificação completa de um grupo específico da Copa.

**Parâmetros:**
- `name` (path): Nome do grupo (ex: "A", "B", "C", etc.)

**Exemplo de Requisição:**
```bash
GET /api/groups/A/standings
Authorization: Bearer {token}
```

**Resposta (200 OK):**
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
      "venue": "Estadio",
      "matchLabel": "Jogo 1",
      "matchday": 1
    }
  ]
}
```

#### GET `/api/groups/standings/all`
Retorna a classificação de todos os grupos da Copa.

**Exemplo de Requisição:**
```bash
GET /api/groups/standings/all
Authorization: Bearer {token}
```

**Resposta (200 OK):**
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
  }
]
```

---

### 2. Ranking em Tempo Real do Bolão

#### GET `/api/ranking/real-time/{groupId}`
Retorna o ranking em tempo real de um grupo do bolão com dados momentâneos e posições dinâmicas.

**Parâmetros:**
- `groupId` (path): ID do grupo do bolão (UUID)

**Exemplo de Requisição:**
```bash
GET /api/ranking/real-time/550e8400-e29b-41d4-a716-446655440000
Authorization: Bearer {token}
```

**Resposta (200 OK):**
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
- `position`: Posição atual no ranking final
- `momentaryPoints`: Pontos que o usuário está ganhando neste momento
- `momentaryPosition`: Posição em que ele ficaria com os pontos momentâneos
- `positionChange`: Quantas posições ele está ganhando/perdendo (positivo = subindo)
- `isLeader`: Se é o líder com os pontos momentâneos
- `pointsDifference`: Diferença de pontos para o líder momentâneo

---

### 3. Ranking Detalhado do Grupo

#### GET `/api/ranking?groupId={groupId}`
Retorna o ranking completo e detalhado de um grupo do bolão.

**Parâmetros:**
- `groupId` (query): ID do grupo do bolão

**Exemplo de Requisição:**
```bash
GET /api/ranking?groupId=550e8400-e29b-41d4-a716-446655440000
Authorization: Bearer {token}
```

**Resposta (200 OK):**
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

## WebSocket (SignalR) - Atualizações em Tempo Real

### Conectar ao Hub

```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://api.example.com/hubs/ranking", {
    accessTokenFactory: () => token
  })
  .withAutomaticReconnect()
  .build();

await connection.start();
```

### Entrar em um Grupo de Ranking

```javascript
// Para acompanhar o ranking em tempo real de um grupo específico
await connection.invoke("JoinGroupRanking", groupId);
```

### Sair de um Grupo de Ranking

```javascript
await connection.invoke("LeaveGroupRanking", groupId);
```

### Entrar no Ranking Global

```javascript
await connection.invoke("JoinGlobalRanking");
```

### Sair do Ranking Global

```javascript
await connection.invoke("LeaveGlobalRanking");
```

### Eventos de Atualização

#### 1. Ranking do Grupo Atualizado
Disparado quando um resultado de jogo é processado e afeta o ranking de um grupo.

```javascript
connection.on("group-ranking-updated", (groupId, matchId) => {
  console.log(`Ranking do grupo ${groupId} atualizado - Match ${matchId}`);
  // Chamar GET /api/ranking/real-time/{groupId} para obter dados atualizados
  fetchRealTimeRanking(groupId);
});
```

#### 2. Ranking Global Atualizado
Disparado quando um resultado de jogo é processado globalmente.

```javascript
connection.on("global-ranking-updated", (matchId) => {
  console.log(`Ranking global atualizado - Match ${matchId}`);
  // Chamar GET /api/ranking para obter dados atualizados
  fetchGlobalRanking();
});
```

#### 3. Rankings Atualizados (Legado)
Disparado para compatibilidade com código existente.

```javascript
connection.on("rankings-updated", (matchId) => {
  console.log(`Resultados processados - Match ${matchId}`);
});
```

---

## Exemplo Completo de Integração Frontend

### React + TypeScript

```typescript
import { useState, useEffect, useRef } from 'react';
import * as signalR from '@microsoft/signalr';

interface RealTimeRankingEntry {
  position: number;
  userId: string;
  userName: string;
  userPhotoUrl: string;
  totalPoints: number;
  exactScores: number;
  correctOutcomes: number;
  totalPredictions: number;
  errors: number;
  momentaryPoints: number;
  momentaryPosition: number;
  positionChange: number;
  isLeader: boolean;
  pointsDifference: number;
  updatedAt: string;
}

interface TeamStanding {
  teamId: number;
  teamName: string;
  fifaCode: string;
  flagUrl: string;
  position: number;
  played: number;
  won: number;
  drawn: number;
  lost: number;
  goalsFor: number;
  goalsAgainst: number;
  goalDifference: number;
  points: number;
}

export function RankingDashboard({ groupId }: { groupId: string }) {
  const [ranking, setRanking] = useState<RealTimeRankingEntry[]>([]);
  const [groupStandings, setGroupStandings] = useState<TeamStanding[]>([]);
  const [loading, setLoading] = useState(true);
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const token = localStorage.getItem('token');

  useEffect(() => {
    // Conectar ao SignalR Hub
    const connection = new signalR.HubConnectionBuilder()
      .withUrl('https://api.example.com/hubs/ranking', {
        accessTokenFactory: () => token || ''
      })
      .withAutomaticReconnect()
      .build();

    connection.start()
      .then(() => {
        console.log('Conectado ao hub');
        connection.invoke('JoinGroupRanking', groupId);
      })
      .catch(err => console.error('Erro ao conectar:', err));

    // Escutar atualizações do ranking
    connection.on('group-ranking-updated', (id, matchId) => {
      if (id === groupId) {
        fetchRealTimeRanking();
      }
    });

    connectionRef.current = connection;

    // Carregar dados iniciais
    fetchRealTimeRanking();

    return () => {
      if (connectionRef.current) {
        connectionRef.current.invoke('LeaveGroupRanking', groupId);
        connectionRef.current.stop();
      }
    };
  }, [groupId, token]);

  const fetchRealTimeRanking = async () => {
    try {
      const response = await fetch(
        `https://api.example.com/api/ranking/real-time/${groupId}`,
        {
          headers: { 'Authorization': `Bearer ${token}` }
        }
      );
      const data = await response.json();
      setRanking(data);
    } catch (error) {
      console.error('Erro ao buscar ranking:', error);
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <div>Carregando...</div>;

  return (
    <div className="ranking-container">
      <h1>Ranking do Bolão</h1>
      <table>
        <thead>
          <tr>
            <th>Posição</th>
            <th>Nome</th>
            <th>Pontos</th>
            <th>Momentaneamente</th>
            <th>Movimento</th>
            <th>Taxa de Acerto</th>
          </tr>
        </thead>
        <tbody>
          {ranking.map((entry) => (
            <tr key={entry.userId} className={entry.isLeader ? 'leader' : ''}>
              <td>
                {entry.momentaryPosition}
                {entry.positionChange > 0 && (
                  <span className="up">↑ {entry.positionChange}</span>
                )}
                {entry.positionChange < 0 && (
                  <span className="down">↓ {Math.abs(entry.positionChange)}</span>
                )}
              </td>
              <td>
                <img src={entry.userPhotoUrl} alt={entry.userName} />
                {entry.userName}
                {entry.isLeader && <span className="badge">🏆 Líder</span>}
              </td>
              <td>
                <strong>{entry.totalPoints}</strong>
                {entry.momentaryPoints > 0 && (
                  <span className="momentary">+{entry.momentaryPoints}</span>
                )}
              </td>
              <td>{entry.momentaryPosition}</td>
              <td>
                {entry.positionChange === 0 ? '—' : entry.positionChange > 0 ? `Subindo ${entry.positionChange}` : `Caindo ${Math.abs(entry.positionChange)}`}
              </td>
              <td>
                {((entry.exactScores * 3 + entry.correctOutcomes) / (entry.totalPredictions * 3) * 100).toFixed(1)}%
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
```

### Estilos CSS

```css
.ranking-container {
  padding: 20px;
  max-width: 1200px;
  margin: 0 auto;
}

.ranking-container h1 {
  color: #333;
  margin-bottom: 20px;
}

table {
  width: 100%;
  border-collapse: collapse;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

thead {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
}

th, td {
  padding: 16px;
  text-align: left;
  border-bottom: 1px solid #e0e0e0;
}

tbody tr:hover {
  background: #f5f5f5;
}

tbody tr.leader {
  background: #fffacd;
}

.momentary {
  display: inline-block;
  color: #4caf50;
  font-weight: bold;
  margin-left: 8px;
  padding: 2px 6px;
  background: #e8f5e9;
  border-radius: 4px;
}

.up {
  color: #4caf50;
  font-weight: bold;
  margin-left: 4px;
}

.down {
  color: #f44336;
  font-weight: bold;
  margin-left: 4px;
}

.badge {
  display: inline-block;
  margin-left: 8px;
  padding: 4px 8px;
  background: #ffd700;
  border-radius: 4px;
  font-weight: bold;
  font-size: 0.9em;
}
```

---

## Fluxo de Atualização em Tempo Real

```
1. Jogo começa (status = InProgress)
   ↓
2. Admin atualiza o resultado parcial
   ↓
3. PredictionService calcula pontos momentâneos
   ↓
4. SignalR envia "group-ranking-updated"
   ↓
5. Frontend recebe evento e busca GET /api/ranking/real-time/{groupId}
   ↓
6. UI exibe:
   - Pontos momentâneos em verde
   - Posição dinâmica
   - Movimento de posição (↑/↓)
   - Indicador de liderança
   ↓
7. Jogo termina (status = Finished)
   ↓
8. PredictionService finaliza pontos
   ↓
9. Rankings são atualizados definitivamente
```

---

## Tratamento de Erros

```typescript
interface ErrorResponse {
  error?: string;
  message?: string;
  statusCode?: number;
}

async function fetchWithErrorHandling(url: string, token: string) {
  try {
    const response = await fetch(url, {
      headers: { 'Authorization': `Bearer ${token}` }
    });

    if (!response.ok) {
      if (response.status === 401) {
        // Token expirado
        window.location.href = '/login';
        return;
      }
      if (response.status === 403) {
        // Não é membro do grupo
        alert('Você não tem permissão para acessar este grupo');
        return;
      }
      if (response.status === 404) {
        // Grupo não encontrado
        alert('Grupo não encontrado');
        return;
      }
    }

    return await response.json();
  } catch (error) {
    console.error('Erro na requisição:', error);
    throw error;
  }
}
```

---

## Performance e Cache

### Estratégia de Cache no Backend
- Classificações são cacheadas por 30 segundos
- Cache é invalidado quando um jogo é finalizado
- Rankings em tempo real não são cacheados (sempre atualizados)

### Recomendações para Frontend
1. Implementar polling com intervalo de 5-10 segundos se SignalR não estiver disponível
2. Armazenar dados em estado local e atualizar apenas mudanças
3. Usar debounce para requisições enquanto houver atualizações
4. Mostrar indicador de "atualizando" enquanto há atualizações em tempo real

```typescript
// Exemplo com debounce
import { useCallback, useEffect } from 'react';

function useRealTimeRanking(groupId: string, delay = 500) {
  const timeoutRef = useRef<NodeJS.Timeout>();

  const debouncedFetch = useCallback(() => {
    clearTimeout(timeoutRef.current);
    timeoutRef.current = setTimeout(() => {
      fetchRealTimeRanking();
    }, delay);
  }, [delay]);

  return { debouncedFetch };
}
```

---

## Verificação de Implementação

✅ **Implementado:**
- Classificação dos grupos com critérios de desempate (pontos, saldo de gols, gols marcados)
- Ranking em tempo real com pontos momentâneos
- Posições dinâmicas calculadas
- WebSocket/SignalR para atualizações
- Cache inteligente (30s para classificações, real-time para ranking)
- Endpoints REST documentados

**Endpoints Disponíveis:**
1. `GET /api/groups/{name}/standings` - Classificação específica
2. `GET /api/groups/standings/all` - Todas as classificações
3. `GET /api/ranking/real-time/{groupId}` - Ranking em tempo real
4. `GET /api/ranking?groupId={groupId}` - Ranking detalhado
5. `WebSocket: /hubs/ranking` - Conexão em tempo real

**Eventos SignalR:**
- `group-ranking-updated` - Ranking do grupo atualizado
- `global-ranking-updated` - Ranking global atualizado
- `rankings-updated` - Compatibilidade (legado)
