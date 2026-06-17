# Prompts para Console Frontend - Bolão Copa 2026

## Configuração Inicial

```javascript
// Defina o token (obtido no login)
const token = 'seu_token_jwt_aqui';
const apiBase = 'https://api.example.com'; // Altere para sua URL

// ID do grupo do bolão para testes
const groupId = '550e8400-e29b-41d4-a716-446655440000'; // Substitua pelo ID real
```

---

## 1. Classificação da Copa

### Obter classificação de um grupo específico

```javascript
const fetchGroupStandings = async (groupName = 'A') => {
  try {
    const response = await fetch(`${apiBase}/api/groups/${groupName}/standings`, {
      headers: { 'Authorization': `Bearer ${token}` }
    });
    const data = await response.json();
    console.log(`📊 Classificação do Grupo ${groupName}:`, data);
    return data;
  } catch (error) {
    console.error('❌ Erro ao buscar classificação:', error);
  }
};

// Executar
fetchGroupStandings('A');
```

**Output esperado:**
```javascript
{
  groupName: 'A',
  teams: [
    {
      teamId: 1,
      teamName: 'Brasil',
      fifaCode: 'BRA',
      flagUrl: '...',
      position: 1,
      played: 3,
      won: 3,
      drawn: 0,
      lost: 0,
      goalsFor: 9,
      goalsAgainst: 1,
      goalDifference: 8,
      points: 9
    },
    // ... mais times
  ],
  matches: [ /* todos os jogos do grupo */ ]
}
```

### Obter classificação de todos os grupos

```javascript
const fetchAllStandings = async () => {
  try {
    const response = await fetch(`${apiBase}/api/groups/standings/all`, {
      headers: { 'Authorization': `Bearer ${token}` }
    });
    const data = await response.json();
    console.log('🏆 Classificação de todos os grupos:', data);
    return data;
  } catch (error) {
    console.error('❌ Erro ao buscar classificações:', error);
  }
};

// Executar
fetchAllStandings();
```

---

## 2. Ranking em Tempo Real do Bolão

### Obter ranking com pontos momentâneos

```javascript
const fetchRealTimeRanking = async () => {
  try {
    const response = await fetch(`${apiBase}/api/ranking/real-time/${groupId}`, {
      headers: { 'Authorization': `Bearer ${token}` }
    });
    const data = await response.json();
    console.log('🔴 RANKING EM TEMPO REAL:', data);
    
    // Exibir resumo formatado
    console.table(data.map(entry => ({
      'Posição': entry.position,
      'Nome': entry.userName,
      'Pontos': entry.totalPoints,
      'Momentâneo': entry.momentaryPoints,
      'Total c/ Momentâneo': entry.totalPoints + entry.momentaryPoints,
      'Posição Momentânea': entry.momentaryPosition,
      'Movimento': entry.positionChange > 0 ? `↑ ${entry.positionChange}` : (entry.positionChange < 0 ? `↓ ${Math.abs(entry.positionChange)}` : '—'),
      'Líder': entry.isLeader ? '🏆' : ''
    })));
    
    return data;
  } catch (error) {
    console.error('❌ Erro ao buscar ranking em tempo real:', error);
  }
};

// Executar
fetchRealTimeRanking();
```

**Output esperado:**
```
┌────────┬────────────────┬───────┬──────────┬─────────────────┬──────────────────┬──────────┬───────┐
│ Posição│    Nome        │Pontos │Momentâneo│Tot c/ Momentâneo│Pos. Momentânea   │ Movimento│ Líder │
├────────┼────────────────┼───────┼──────────┼─────────────────┼──────────────────┼──────────┼───────┤
│   1    │ João Silva     │  45   │    3     │       48        │        1         │    —     │  🏆  │
│   2    │ Maria Santos   │  42   │    4     │       46        │        1         │   ↑ 1   │      │
│   3    │ Pedro Oliveira │  40   │    1     │       41        │        3         │   ↓ 1   │      │
└────────┴────────────────┴───────┴──────────┴─────────────────┴──────────────────┴──────────┴───────┘
```

---

## 3. Ranking Detalhado do Grupo

### Obter ranking com métricas detalhadas

```javascript
const fetchDetailedGroupRanking = async () => {
  try {
    const response = await fetch(`${apiBase}/api/ranking?groupId=${groupId}`, {
      headers: { 'Authorization': `Bearer ${token}` }
    });
    const data = await response.json();
    console.log('📈 RANKING DETALHADO DO GRUPO:', data);
    
    console.log(`Grupo: ${data.groupName}`);
    console.log(`Membros: ${data.totalMembers}`);
    console.log(`Jogos: ${data.processedMatches}/${data.totalMatches} processados`);
    
    console.table(data.rankings.map(entry => ({
      'Posição': entry.position,
      'Nome': entry.userName,
      'Pontos': entry.totalPoints,
      'Acertos Exatos': entry.exactScores,
      'Acertos Parciais': entry.correctOutcomes,
      'Pts/Pred': entry.pointsPerPrediction,
      'Acurácia': entry.accuracyRate + '%',
      'Líder': entry.isLeader ? '👑' : ''
    })));
    
    return data;
  } catch (error) {
    console.error('❌ Erro ao buscar ranking detalhado:', error);
  }
};

// Executar
fetchDetailedGroupRanking();
```

---

## 4. Conectar ao SignalR para Atualizações em Tempo Real

### Setupar conexão WebSocket

```javascript
// Se usando npm: npm install @microsoft/signalr
// Ou link CDN: <script src="https://cdn.jsdelivr.net/npm/@microsoft/signalr@latest/signalr.min.js"></script>

let connection;

const setupSignalRConnection = async () => {
  connection = new signalR.HubConnectionBuilder()
    .withUrl(`${apiBase}/hubs/ranking`, {
      accessTokenFactory: () => token
    })
    .withAutomaticReconnect()
    .build();

  // Evento: Ranking do grupo foi atualizado
  connection.on('group-ranking-updated', (gId, matchId) => {
    console.log(`🔄 Ranking do grupo atualizado! Match: ${matchId}`);
    if (gId === groupId) {
      console.log('⚡ Refetchando ranking...');
      fetchRealTimeRanking();
    }
  });

  // Evento: Ranking global foi atualizado
  connection.on('global-ranking-updated', (matchId) => {
    console.log(`🌍 Ranking global atualizado! Match: ${matchId}`);
  });

  // Evento: Rankings atualizados (compatibilidade)
  connection.on('rankings-updated', (matchId) => {
    console.log(`✅ Resultados do match ${matchId} foram processados`);
  });

  try {
    await connection.start();
    console.log('✅ Conectado ao SignalR');
    
    // Entrar no grupo
    await connection.invoke('JoinGroupRanking', groupId);
    console.log(`✅ Entrado no grupo de ranking: ${groupId}`);
    
    // Opcional: entrar no ranking global
    await connection.invoke('JoinGlobalRanking');
    console.log('✅ Entrado no ranking global');
    
  } catch (error) {
    console.error('❌ Erro ao conectar ao SignalR:', error);
  }
};

// Executar
setupSignalRConnection();
```

### Desconectar

```javascript
const disconnectSignalR = async () => {
  if (connection) {
    try {
      await connection.invoke('LeaveGroupRanking', groupId);
      await connection.invoke('LeaveGlobalRanking');
      await connection.stop();
      console.log('✅ Desconectado do SignalR');
    } catch (error) {
      console.error('❌ Erro ao desconectar:', error);
    }
  }
};

// Executar quando terminar
disconnectSignalR();
```

---

## 5. Implementação Completa de Monitoramento

```javascript
// Dashboard em tempo real com refresh automático
class RankingDashboard {
  constructor(apiBase, token, groupId) {
    this.apiBase = apiBase;
    this.token = token;
    this.groupId = groupId;
    this.isMonitoring = false;
    this.refreshInterval = 5000; // 5 segundos
  }

  async start() {
    console.log('🎯 Iniciando dashboard de ranking...');
    this.isMonitoring = true;
    
    // Setup SignalR
    await this.setupSignalR();
    
    // Fetch inicial
    await this.refresh();
    
    // Refresh periódico como fallback
    setInterval(() => this.refresh(), this.refreshInterval);
  }

  async setupSignalR() {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${this.apiBase}/hubs/ranking`, {
        accessTokenFactory: () => this.token
      })
      .withAutomaticReconnect()
      .build();

    this.connection.on('group-ranking-updated', (gId, matchId) => {
      if (gId === this.groupId) {
        console.log(`📨 Evento recebido do servidor - Match ${matchId}`);
        this.refresh();
      }
    });

    await this.connection.start();
    await this.connection.invoke('JoinGroupRanking', this.groupId);
    console.log('✅ SignalR conectado');
  }

  async refresh() {
    try {
      const response = await fetch(`${this.apiBase}/api/ranking/real-time/${this.groupId}`, {
        headers: { 'Authorization': `Bearer ${this.token}` }
      });
      const data = await response.json();
      
      console.clear();
      console.log('╔════════════════════════════════════════════════════╗');
      console.log('║       RANKING DO BOLÃO - TEMPO REAL               ║');
      console.log('╚════════════════════════════════════════════════════╝');
      
      data.forEach((entry, index) => {
        const totalComMomentaneo = entry.totalPoints + entry.momentaryPoints;
        const arrow = entry.positionChange > 0 ? '📈' : entry.positionChange < 0 ? '📉' : '➡️';
        const leader = entry.isLeader ? ' 👑' : '';
        
        console.log(
          `${index + 1}. ${entry.userName}${leader}\n` +
          `   Pontos: ${entry.totalPoints} ${entry.momentaryPoints > 0 ? `(+${entry.momentaryPoints})` : ''}${entry.momentaryPoints > 0 ? ' ⚡' : ''}\n` +
          `   Pos. Momentânea: ${entry.momentaryPosition} ${arrow}\n` +
          `   Acertos: ${entry.exactScores}/${entry.correctOutcomes}/${entry.errors}\n`
        );
      });
      
      console.log(`\n⏰ Atualizado às: ${new Date().toLocaleTimeString('pt-BR')}`);
      console.log(`🔄 Próxima atualização em ${this.refreshInterval / 1000}s`);
    } catch (error) {
      console.error('❌ Erro ao atualizar:', error);
    }
  }

  async stop() {
    this.isMonitoring = false;
    if (this.connection) {
      await this.connection.invoke('LeaveGroupRanking', this.groupId);
      await this.connection.stop();
    }
    console.log('⏹️ Dashboard parado');
  }
}

// Usar:
const dashboard = new RankingDashboard(apiBase, token, groupId);
dashboard.start();

// Parar:
// dashboard.stop();
```

---

## 6. Testes de Carga

```javascript
// Testar múltiplos grupos simultaneamente
const testMultipleGroups = async (groupIds) => {
  console.time('⏱️ Tempo total');
  
  const promises = groupIds.map(gId =>
    fetch(`${apiBase}/api/ranking/real-time/${gId}`, {
      headers: { 'Authorization': `Bearer ${token}` }
    }).then(r => r.json())
  );
  
  const results = await Promise.all(promises);
  
  results.forEach((data, idx) => {
    console.log(`📊 Grupo ${idx + 1}: ${data.length} participantes`);
  });
  
  console.timeEnd('⏱️ Tempo total');
};

// Exemplo:
// testMultipleGroups([groupId1, groupId2, groupId3]);
```

---

## 7. Debug & Troubleshooting

```javascript
// Verificar saúde da API
const healthCheck = async () => {
  try {
    const response = await fetch(`${apiBase}/api/groups`, {
      headers: { 'Authorization': `Bearer ${token}` }
    });
    console.log('✅ API respondendo:', response.status);
    return true;
  } catch (error) {
    console.error('❌ API não está respondendo:', error);
    return false;
  }
};

// Verificar token
const verifyToken = () => {
  try {
    const parts = token.split('.');
    if (parts.length !== 3) throw new Error('Token inválido');
    
    const payload = JSON.parse(atob(parts[1]));
    const expiry = new Date(payload.exp * 1000);
    
    console.log('✅ Token válido');
    console.log('Expira em:', expiry.toLocaleString('pt-BR'));
    return true;
  } catch (error) {
    console.error('❌ Erro no token:', error);
    return false;
  }
};

// Executar diagnóstico
const runDiagnostics = async () => {
  console.log('🔍 Iniciando diagnóstico...\n');
  
  console.log('1️⃣ Verificando token...');
  verifyToken();
  
  console.log('\n2️⃣ Verificando API...');
  await healthCheck();
  
  console.log('\n3️⃣ Buscando classificação...');
  await fetchGroupStandings('A');
  
  console.log('\n✅ Diagnóstico concluído!');
};

// runDiagnostics();
```

---

## Resumo dos Endpoints

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/groups/{name}/standings` | Classificação de um grupo |
| GET | `/api/groups/standings/all` | Todas as classificações |
| GET | `/api/ranking/real-time/{groupId}` | Ranking em tempo real |
| GET | `/api/ranking?groupId={groupId}` | Ranking detalhado |
| WS | `/hubs/ranking` | WebSocket para atualizações |

---

## Checklist de Implementação no Frontend

- [ ] Implementar conexão SignalR
- [ ] Criar componente de Classificação dos Grupos
- [ ] Criar componente de Ranking em Tempo Real
- [ ] Adicionar indicadores visuais (↑/↓, cores, badges)
- [ ] Implementar sistema de notificações
- [ ] Testar em conexão lenta/offline
- [ ] Adicionar tratamento de erros
- [ ] Implementar loading states
- [ ] Otimizar renderização (useCallback, memo)
- [ ] Implementar analytics/logging
