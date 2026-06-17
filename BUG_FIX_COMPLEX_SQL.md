# 🐛 Fix v2 - Reescrever Query Complexa

## ❌ Problema (Segunda Vez)

Mesmo após a primeira correção, continuava recebendo:
```
Incorrect syntax near the keyword 'WITH'
```

---

## 🔍 Causa Real

O problema era na query de `GetGroupRankingAsync` que tinha múltiplas agregações:

```csharp
// ❌ ERRADO - Gera SQL muito complexo
var raw = await context.Users
    .Where(u => memberIds.Contains(u.Id) && u.IsActive)
    .Select(u => new
    {
        u.Id, u.Name, u.PhotoUrl,
        TotalPoints = u.Predictions.Where(p => p.IsProcessed && p.GroupId == groupId).Sum(p => p.Points),
        ExactScores = u.Predictions.Count(p => p.IsProcessed && p.GroupId == groupId && p.Points == 3),
        CorrectOutcomes = u.Predictions.Count(p => p.IsProcessed && p.GroupId == groupId && p.Points == 1),
        TotalPredictions = u.Predictions.Count(p => p.GroupId == groupId),
        Errors = u.Predictions.Count(p => p.IsProcessed && p.GroupId == groupId && p.Points == 0)
    })
    .OrderByDescending(u => u.TotalPoints)
    .ThenByDescending(u => u.ExactScores)
    .ThenByDescending(u => u.CorrectOutcomes)
    .ThenBy(u => u.Name)
    .ToListAsync();
```

**Problema:** Cada agregação gera uma subquery, e SQL Server não consegue otimizar essa query complexa.

---

## ✅ Solução

Mover processamento para memória:

```csharp
// ✅ CORRETO - SQL simples, processamento em memória
var users = await context.Users
    .Where(u => memberIds.Contains(u.Id) && u.IsActive)
    .Include(u => u.Predictions)
    .ToListAsync();

var raw = users
    .Select(u => new
    {
        u.Id,
        u.Name,
        u.PhotoUrl,
        Predictions = u.Predictions.Where(p => p.GroupId == groupId).ToList()
    })
    .Select(u => new
    {
        u.Id,
        u.Name,
        u.PhotoUrl,
        TotalPoints = u.Predictions.Where(p => p.IsProcessed).Sum(p => p.Points),
        ExactScores = u.Predictions.Count(p => p.IsProcessed && p.Points == 3),
        CorrectOutcomes = u.Predictions.Count(p => p.IsProcessed && p.Points == 1),
        TotalPredictions = u.Predictions.Count(),
        Errors = u.Predictions.Count(p => p.IsProcessed && p.Points == 0)
    })
    .OrderByDescending(u => u.TotalPoints)
    .ThenByDescending(u => u.ExactScores)
    .ThenByDescending(u => u.CorrectOutcomes)
    .ThenBy(u => u.Name)
    .ToList();
```

**Diferença:**
1. **Antes:** Tudo na query SQL (complexo demais)
2. **Depois:** Dados vêm simples, processamento em LINQ to Objects (simples)

---

## 🔄 Commits

| Commit | Descrição |
|--------|-----------|
| `0857c85` | Primeira tentativa de fix |
| `b7baf7d` | Documentação da primeira correção |
| `b249da6` | Segunda correção - Reescrever query |

---

## 🧪 Teste Novamente

```bash
# Via CURL
curl -X GET "https://localhost:5001/api/ranking/real-time/{groupId}" \
  -H "Authorization: Bearer {token}"

# Esperado: 200 OK com array de rankings
```

---

## ✨ O que mudou

- ✅ SQL query agora é simples e direta
- ✅ Processamento de agregações em memória (rápido)
- ✅ Sem CTEs ou subqueries complexas
- ✅ Compatível com SQL Server

**Status:** 🟢 **Pronto para testar novamente!**
