# Integração Frontend - Sistema de Ranking em Tempo Real

## 📡 Novo Endpoint WebSocket (SignalR)

A API agora suporta atualizações em tempo real via WebSocket usando **SignalR**.

### Conexão ao Hub

```javascript
// JavaScript/TypeScript
const hubConnection = new signalR.HubConnectionBuilder()
  .withUrl("https://api.seu-servidor.com/hubs/ranking", {
    accessTokenFactory: () => localStorage.getItem("authToken") // JWT token
  })
  .withAutomaticReconnect()
  .build();

hubConnection.start()
  .then(() => console.log("Conectado ao hub de ranking"))
  .catch(err => console.error("Erro ao conectar:", err));

// Escutar evento de atualização
hubConnection.on("rankings-updated", (matchId) => {
  console.log(`Ranking atualizado! Jogo ${matchId} foi processado.`);
  // Aqui você deve buscar o ranking atualizado
  fetchRanking();
});

// Desconectar quando necessário
await hubConnection.stop();
```

### Vue 3 + Pinia (Exemplo)

```typescript
// stores/rankingStore.ts
import { defineStore } from 'pinia';
import * as signalR from "@aspnet/signalr";
import { ref } from 'vue';

export const useRankingStore = defineStore('ranking', () => {
  const ranking = ref([]);
  const hubConnection = ref(null);

  const connectHub = async (token: string) => {
    hubConnection.value = new signalR.HubConnectionBuilder()
      .withUrl("/hubs/ranking", {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    hubConnection.value.on("rankings-updated", async (matchId) => {
      console.log(`🔄 Ranking atualizado (Jogo ${matchId})`);
      await fetchRanking(); // Busca novo ranking
    });

    await hubConnection.value.start();
  };

  const fetchRanking = async () => {
    const response = await fetch('/api/ranking', {
      headers: { 'Authorization': `Bearer ${token}` }
    });
    ranking.value = await response.json();
  };

  const disconnectHub = async () => {
    if (hubConnection.value) {
      await hubConnection.value.stop();
    }
  };

  return { ranking, connectHub, fetchRanking, disconnectHub };
});
```

### React Hook (Exemplo)

```typescript
// hooks/useRankingHub.ts
import { useEffect, useState } from 'react';
import * as signalR from "@aspnet/signalr";

export const useRankingHub = (token: string) => {
  const [ranking, setRanking] = useState([]);
  const [connection, setConnection] = useState(null);

  useEffect(() => {
    const hubConnection = new signalR.HubConnectionBuilder()
      .withUrl("/hubs/ranking", {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    hubConnection.on("rankings-updated", async (matchId) => {
      console.log(`🔄 Ranking atualizado (Jogo ${matchId})`);
      await fetchRanking();
    });

    hubConnection.start()
      .then(() => setConnection(hubConnection))
      .catch(err => console.error("Erro ao conectar:", err));

    return () => hubConnection.stop();
  }, [token]);

  const fetchRanking = async () => {
    const response = await fetch('/api/ranking', {
      headers: { 'Authorization': `Bearer ${token}` }
    });
    const data = await response.json();
    setRanking(data);
  };

  return { ranking, fetchRanking };
};
```

---

## 🔄 Fluxo de Atualização

### Antes (Polling)
```
[Cliente]  →  GET /api/ranking (a cada 5-10s)
                      ↓
                  [API recalcula ranking]
                      ↓
              [Cliente renderiza]
```

### Depois (WebSocket)
```
[Cliente] conecta WebSocket → [Hub SignalR]
                    ↓
            [Processamento de pontos]
                    ↓
            Hub emite "rankings-updated"
                    ↓
   [Cliente recebe evento] → Busca ranking atualizado
```

---

## 📋 Endpoints REST (sem mudanças)

### GET /api/ranking
```bash
curl -H "Authorization: Bearer {token}" \
  https://api.seu-servidor.com/api/ranking
```

**Resposta** (mais rápida agora com cache):
```json
[
  {
    "position": 1,
    "userId": "uuid",
    "userName": "João",
    "userPhotoUrl": "https://...",
    "totalPoints": 42,
    "exactScores": 8,
    "correctOutcomes": 12,
    "totalPredictions": 30
  },
  ...
]
```

### GET /api/ranking/me
```bash
curl -H "Authorization: Bearer {token}" \
  https://api.seu-servidor.com/api/ranking/me
```

**Resposta**:
```json
{
  "position": 3,
  "userId": "uuid",
  "userName": "Você",
  "userPhotoUrl": "https://...",
  "totalPoints": 35,
  "exactScores": 6,
  "correctOutcomes": 10,
  "totalPredictions": 30
}
```

### GET /api/bolao-groups/{groupId}/ranking
```bash
curl -H "Authorization: Bearer {token}" \
  https://api.seu-servidor.com/api/bolao-groups/{groupId}/ranking
```

---

## ⚡ Otimizações Implementadas

1. **IMemoryCache** (60s TTL)
   - Ranking global cacheado
   - Ranking por grupo cacheado
   - Cache invalidado automaticamente quando pontos são processados

2. **Índices no BD**
   - `(GroupId, IsProcessed)` — acelera queries de ranking por grupo
   - `(MatchId, IsProcessed)` — acelera processamento de pontos
   - **Impacto**: ~5-10x mais rápido em grandes volumes

3. **SignalR (Tempo Real)**
   - Zero latência de atualização
   - Reconexão automática
   - Suporta múltiplos clientes simultâneos

---

## 🔧 Checklist de Implementação Frontend

- [ ] Instalar cliente SignalR: `npm install @aspnet/signalr`
- [ ] Criar conexão ao hub `/hubs/ranking`
- [ ] Implementar listener para `"rankings-updated"`
- [ ] Substituir polling de ranking (se existente)
- [ ] Testar reconexão automática do WebSocket
- [ ] Adicionar indicador visual "⚡ Atualização em tempo real"
- [ ] Testar com múltiplas abas abertas (eventos devem ser recebidos em todas)
- [ ] Remover intervalos de `setInterval()` para ranking
- [ ] Adicionar logging de eventos para debug

---

## 🚨 Notas Importantes

### Autenticação
- O WebSocket **requer JWT token válido** via `accessTokenFactory`
- Se o token expirar, a reconexão automática irá falhar
- Implemente refresh de token no cliente

### CORS
- O servidor agora permite `.AllowCredentials()` para SignalR
- Use `withCredentials: true` ao conectar (já configurado automaticamente)

### Reconexão
- SignalR reconecta automaticamente com backoff exponencial
- Configure `.withAutomaticReconnect()` com timings apropriados (default: 0, 2, 10, 30s, depois falha)

### Múltiplas Abas
- O evento é broadcast para **todos os clientes**
- Não há sincronização cliente-a-cliente (apenas servidor)
- Cada aba deve ter sua própria conexão WebSocket

---

## 📊 Exemplo Completo: Componente Vue

```vue
<template>
  <div class="ranking-container">
    <div v-if="!isConnected" class="warning">
      ⚠️ Desconectado do servidor de atualizações
    </div>
    
    <div v-if="isConnected" class="status">
      ⚡ Atualizações em tempo real ativas
    </div>

    <table>
      <thead>
        <tr>
          <th>Posição</th>
          <th>Jogador</th>
          <th>Pontos</th>
          <th>Acertos Exatos</th>
          <th>Resultados Corretos</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="entry in ranking" :key="entry.userId">
          <td>{{ entry.position }}</td>
          <td>
            <img :src="entry.userPhotoUrl" :alt="entry.userName" />
            {{ entry.userName }}
          </td>
          <td><strong>{{ entry.totalPoints }}</strong></td>
          <td>{{ entry.exactScores }}</td>
          <td>{{ entry.correctOutcomes }}</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted, ref, computed } from 'vue';
import { useRankingStore } from '@/stores/rankingStore';

const rankingStore = useRankingStore();
const token = computed(() => localStorage.getItem('authToken'));
const isConnected = ref(false);

onMounted(async () => {
  if (token.value) {
    const hubConnection = await rankingStore.connectHub(token.value);
    isConnected.value = true;
    
    // Busca inicial
    await rankingStore.fetchRanking();
  }
});

onUnmounted(async () => {
  await rankingStore.disconnectHub();
  isConnected.value = false;
});

const ranking = computed(() => rankingStore.ranking);
</script>

<style scoped>
.warning {
  background: #fff3cd;
  border: 1px solid #ffc107;
  color: #856404;
  padding: 12px;
  border-radius: 4px;
  margin-bottom: 16px;
}

.status {
  background: #d4edda;
  border: 1px solid #28a745;
  color: #155724;
  padding: 12px;
  border-radius: 4px;
  margin-bottom: 16px;
}

table {
  width: 100%;
  border-collapse: collapse;
}

th, td {
  padding: 12px;
  text-align: left;
  border-bottom: 1px solid #ddd;
}

th {
  background: #f5f5f5;
  font-weight: bold;
}
</style>
```

---

## 🆘 Troubleshooting

| Problema | Solução |
|----------|---------|
| WebSocket não conecta | Verifique JWT token válido, URL correta, CORS |
| Eventos não chegam | Abra DevTools → Network → WS, procure por `/hubs/ranking` |
| Reconexão infinita | Token expirou, implemente refresh automático |
| Cache antigo demais | Cache expira em 60s, aguarde ou force refresh |

---

## 📞 Suporte

Para dúvidas sobre a integração:
- Verifique o endpoint `/hubs/ranking` está respondendo
- Teste a conexão: `curl -i https://api.seu-servidor.com/hubs/ranking`
- Logs do navegador (F12 → Console) para debug
