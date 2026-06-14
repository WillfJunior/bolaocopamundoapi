# Endpoint de Recuperação de Senha - Prompt para Frontend

## 📋 Resumo da Funcionalidade

Implementar tela de recuperação de senha onde o usuário insere seu telefone e recebe uma senha temporária via **push notification**. A senha é visível no push, mas nunca é armazenada em plain text no banco de dados.

---

## 🔗 Detalhes do Endpoint

### Rota
```http
POST /api/auth/forgot-password
```

### Request
```json
{
  "phoneNumber": "11987654321"
}
```

### Response (200 OK)
```json
{
  "message": "Senha temporária enviada para você via notificação push.",
  "userName": "João Silva"
}
```

### Erros
- **404 Not Found**: Usuário com esse telefone não encontrado
- **500 Internal Server Error**: Erro ao gerar ou enviar a senha

---

## 🎯 Comportamento Esperado

### O que o Backend faz:
1. Busca usuário pelo telefone (ativo)
2. Gera uma senha temporária aleatória de 12 caracteres
3. Faz hash da senha com BCrypt e salva no banco
4. Envia push notification com a senha visível
5. **Nunca salva a senha em plain text**

### O que o Frontend precisa fazer:
1. Criar tela de "Esqueceu a Senha?"
2. Input para telefone
3. Chamar o endpoint
4. Mostrar feedback ao usuário
5. Ao sucesso: mostrar mensagem e redirecionar para login

---

## 🛠️ Implementação no React

### 1. Criar o Hook/Service para chamar o endpoint

```typescript
// services/authService.ts
export const forgotPassword = async (phoneNumber: string) => {
  const response = await fetch('/api/auth/forgot-password', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ phoneNumber })
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message || 'Erro ao recuperar senha');
  }

  return await response.json();
};
```

### 2. Criar Componente de Recuperação de Senha

```typescript
// pages/ForgotPasswordPage.tsx
import { useState } from 'react';
import { forgotPassword } from '../services/authService';
import { useNavigate } from 'react-router-dom';

export function ForgotPasswordPage() {
  const [phoneNumber, setPhoneNumber] = useState('');
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState('');
  const [userName, setUserName] = useState('');
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!phoneNumber.trim()) {
      setError('Por favor, insira seu telefone');
      return;
    }

    setLoading(true);
    setError('');

    try {
      const result = await forgotPassword(phoneNumber);
      setUserName(result.userName);
      setSuccess(true);

      // Redirecionar para login após 3 segundos
      setTimeout(() => {
        navigate('/login');
      }, 3000);
    } catch (err) {
      setError(
        err instanceof Error 
          ? err.message 
          : 'Erro ao recuperar senha. Tente novamente.'
      );
    } finally {
      setLoading(false);
    }
  };

  if (success) {
    return (
      <div className="forgot-password-success">
        <div className="success-icon">✅</div>
        <h2>Sucesso!</h2>
        <p>
          Olá <strong>{userName}</strong>, sua senha temporária foi enviada
          para você via notificação push.
        </p>
        <p className="info-text">
          Você será redirecionado para a tela de login em alguns segundos...
        </p>
        <button 
          onClick={() => navigate('/login')}
          className="btn-login"
        >
          Ir para Login Agora
        </button>
      </div>
    );
  }

  return (
    <div className="forgot-password-container">
      <form onSubmit={handleSubmit} className="forgot-password-form">
        <h1>Recuperar Senha</h1>
        <p className="subtitle">
          Insira seu número de telefone e receberemos uma senha temporária
          via notificação push.
        </p>

        {error && (
          <div className="error-message" role="alert">
            ❌ {error}
          </div>
        )}

        <div className="form-group">
          <label htmlFor="phone">Número de Telefone</label>
          <input
            id="phone"
            type="tel"
            placeholder="(11) 98765-4321"
            value={phoneNumber}
            onChange={(e) => setPhoneNumber(e.target.value)}
            disabled={loading}
            required
          />
          <small>Insira o mesmo telefone que usou ao se registrar</small>
        </div>

        <button 
          type="submit" 
          disabled={loading || !phoneNumber.trim()}
          className="btn-primary"
        >
          {loading ? '⏳ Enviando...' : '📤 Enviar Senha'}
        </button>

        <button 
          type="button"
          onClick={() => navigate('/login')}
          className="btn-secondary"
          disabled={loading}
        >
          Voltar para Login
        </button>
      </form>

      <div className="info-box">
        <h3>⚡ Como funciona:</h3>
        <ol>
          <li>Insira seu número de telefone</li>
          <li>Clique em "Enviar Senha"</li>
          <li>Você receberá uma notificação push com sua senha temporária</li>
          <li>Faça login com a senha temporária</li>
          <li>Altere para uma senha de sua preferência</li>
        </ol>
      </div>
    </div>
  );
}
```

### 3. Integrar na Navegação (Login Page)

Na página de login, adicione um link:

```typescript
// pages/LoginPage.tsx
export function LoginPage() {
  const navigate = useNavigate();

  return (
    <div className="login-container">
      {/* ... campos de login ... */}
      
      <div className="forgot-password-link">
        <button 
          type="button"
          onClick={() => navigate('/forgot-password')}
          className="link-button"
        >
          Esqueceu a senha?
        </button>
      </div>
    </div>
  );
}
```

### 4. Configurar Rotas

```typescript
// App.tsx ou routes.tsx
import { ForgotPasswordPage } from './pages/ForgotPasswordPage';

const routes = [
  {
    path: '/login',
    element: <LoginPage />
  },
  {
    path: '/forgot-password',
    element: <ForgotPasswordPage />
  },
  // ... outras rotas ...
];
```

---

## 🎨 Exemplo de Styling (CSS/TailwindCSS)

### Com Tailwind CSS

```jsx
<div className="min-h-screen bg-gradient-to-b from-blue-50 to-white flex items-center justify-center p-4">
  <div className="w-full max-w-md bg-white rounded-lg shadow-lg p-8">
    <h1 className="text-2xl font-bold text-center text-gray-800 mb-2">
      Recuperar Senha
    </h1>
    <p className="text-center text-gray-600 mb-6">
      Insira seu número de telefone para receber uma senha temporária
    </p>

    {error && (
      <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded mb-4">
        {error}
      </div>
    )}

    <form onSubmit={handleSubmit} className="space-y-4">
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Telefone
        </label>
        <input
          type="tel"
          placeholder="(11) 98765-4321"
          value={phoneNumber}
          onChange={(e) => setPhoneNumber(e.target.value)}
          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
        />
      </div>

      <button
        type="submit"
        disabled={loading}
        className="w-full bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 rounded-lg disabled:opacity-50"
      >
        {loading ? 'Enviando...' : 'Enviar Senha'}
      </button>

      <button
        type="button"
        onClick={() => navigate('/login')}
        className="w-full text-gray-600 hover:text-gray-800 py-2"
      >
        Voltar
      </button>
    </form>
  </div>
</div>
```

---

## 📱 Fluxo Visual do Usuário

```
┌─────────────────────┐
│   Login Page        │
│  [Telefone input]   │
│  [Senha input]      │
│  [Entrar]           │
│  [Esqueceu senha?]◄─┐
└─────────────────────┘│
                       │
┌─────────────────────────────┐
│   Forgot Password Page    ◄──┘
│  [Telefone input]           │
│  [Enviar Senha]             │
└─────────────────────────────┘
           ↓ (clica Enviar)
┌─────────────────────────────┐
│   Loading...                │
│   (enviando para backend)   │
└─────────────────────────────┘
           ↓ (sucesso)
┌─────────────────────────────┐
│   ✅ Sucesso!              │
│   Senha enviada para:       │
│   João Silva                │
│   Verifique seu push...     │
│   (redireciona em 3s)       │
└─────────────────────────────┘
           ↓
┌─────────────────────┐
│   Login Page        │
│   User tenta fazer  │
│   login com a       │
│   senha temporária  │
└─────────────────────┘
```

---

## ✅ Checklist de Implementação

- [ ] Criar componente `ForgotPasswordPage`
- [ ] Implementar serviço `forgotPassword()`
- [ ] Criar rota `/forgot-password`
- [ ] Adicionar link "Esqueceu a senha?" no login
- [ ] Implementar validação de telefone
- [ ] Mostrar mensagem de erro/sucesso
- [ ] Redirecionar automático após sucesso
- [ ] Testar fluxo completo
- [ ] Testar recebimento de push notification
- [ ] Verificar login com senha temporária
- [ ] Verificar alteração de senha após primeiro login

---

## 🔍 Testes Recomendados

### Caso de Sucesso
1. Inserir telefone válido
2. Receber push com senha
3. Fazer login com senha temporária
4. Alterar para nova senha

### Casos de Erro
1. Telefone não encontrado → erro 404
2. Telefone inválido → erro no frontend
3. Campo vazio → desabilitar botão
4. Sem push subscription → mensagem de aviso (opcional)

---

## 📝 Notas Importantes

⚠️ **Senha visível no push**: A senha é mostrada no push porque precisa ser visível ao usuário. Não há risco de segurança pois:
- Push é enviado apenas para o usuário (via PushSubscription dele)
- A senha é temporária
- Usuário deve alterar após primeiro login
- Nunca é armazenada em plain text no banco

✅ **Melhor UX**: 
- Mostrar loading enquanto aguarda resposta
- Redirecionar automático para login após sucesso
- Validar telefone antes de enviar
- Mensagens de erro claras

---

**Pronto para implementar!** 🚀
