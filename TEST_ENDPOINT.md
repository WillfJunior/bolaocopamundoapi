# 🧪 Teste o Endpoint - Ranking em Tempo Real

## Via CURL

```bash
curl -X GET "https://localhost:5001/api/ranking/real-time/550e8400-e29b-41d4-a716-446655440000" \
  -H "Authorization: Bearer {SEU_TOKEN_JWT}" \
  -H "Content-Type: application/json"
```

**Substituir:**
- `550e8400-e29b-41d4-a716-446655440000` → ID real do seu grupo
- `{SEU_TOKEN_JWT}` → Seu token JWT obtido no login

---

## Via Postman

1. **Nova Request** → GET
2. **URL:** `https://localhost:5001/api/ranking/real-time/{groupId}`
3. **Headers:**
   - `Authorization: Bearer {token}`
4. **Send**

---

## Via Browser Console

```javascript
const token = localStorage.getItem('token'); // ou seu token
const groupId = 'seu-group-id'; // replace com ID real

fetch(`https://localhost:5001/api/ranking/real-time/${groupId}`, {
  headers: { 'Authorization': `Bearer ${token}` }
})
.then(r => r.json())
.then(data => console.log('✅ Sucesso:', data))
.catch(e => console.error('❌ Erro:', e));
```

---

## ✅ Resposta Esperada (200 OK)

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
  }
]
```

---

## ❌ Se Receber Erro

### 401 - Unauthorized
```
Problema: Token não foi enviado ou é inválido
Solução: Verificar se Authorization header está correto
```

### 403 - Forbidden
```
Problema: Você não é membro do grupo
Solução: Entrar no grupo primeiro
```

### 404 - Not Found
```
Problema: Grupo não existe
Solução: Verificar se groupId está correto
```

### 500 - Server Error
```
Problema: Erro na API
Solução: Verificar logs da API
```

---

## ✨ Status

**Último Fix:** Corrigido SQL syntax error
**Build:** ✅ Success
**Ready:** 🟢 Yes

Teste agora! 🚀
