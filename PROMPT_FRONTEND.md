# Prompt — Frontend PWA: Bolão Copa do Mundo 2026

## Contexto

Crie o frontend de um **bolão da Copa do Mundo 2026** como uma **PWA (Progressive Web App)** que consome uma API REST já existente em ASP.NET Core.
O usuário se cadastra com telefone + senha, cria ou entra em grupos de bolão via link de convite (compartilhável pelo WhatsApp), faz palpites nos jogos de cada grupo da Copa, acompanha a classificação geral e por grupo em tempo real, e recebe notificações push.

---

## Stack obrigatória

| Camada | Tecnologia |
|---|---|
| Framework | **React 18** + **TypeScript** + **Vite** |
| PWA | **vite-plugin-pwa** (Workbox) |
| Roteamento | **React Router v6** |
| Data fetching | **TanStack Query v5** (React Query) |
| HTTP | **Axios** com interceptor JWT |
| Formulários | **React Hook Form** + **Zod** |
| Estilos | **Tailwind CSS** + **shadcn/ui** |
| Estado global | **Zustand** (auth + user) |
| Push notifications | Web Push API + Service Worker |
| Ícones | **Lucide React** |
| Datas | **date-fns** + **date-fns-tz** com locale `pt-BR` |

---

## Setup do projeto

```bash
npm create vite@latest bolao-copa -- --template react-ts
cd bolao-copa
npm install react-router-dom @tanstack/react-query axios react-hook-form zod @hookform/resolvers zustand date-fns date-fns-tz lucide-react react-hot-toast
npm install -D tailwindcss postcss autoprefixer vite-plugin-pwa @types/node
npx tailwindcss init -p
npx shadcn-ui@latest init
```

---

## Variáveis de ambiente

```env
VITE_API_BASE_URL=http://localhost:5196
VITE_VAPID_PUBLIC_KEY=   # buscado de GET /api/notifications/vapid-public-key
```

---

## Tipos TypeScript (src/types/api.ts)

```ts
// ─── Auth ───────────────────────────────────────────────────────────────
export interface AuthResponse {
  token: string;
  expiresAt: string;
  user: UserInfo;
}
export interface UserInfo {
  id: string; name: string; phoneNumber: string;
  photoUrl: string | null; isAdmin: boolean;
}

// ─── Usuário ────────────────────────────────────────────────────────────
export interface UserDto {
  id: string; name: string; phoneNumber: string;
  photoUrl: string | null; isAdmin: boolean; createdAt: string;
}

// ─── Copa do Mundo ─────────────────────────────────────────────────────
export interface TeamDto { id: number; name: string; fifaCode: string; flagUrl: string | null; }
export interface MatchDto {
  id: number; homeTeam: TeamDto | null; awayTeam: TeamDto | null;
  groupName: string | null; phase: MatchPhase; status: MatchStatus;
  matchDate: string; homeScore: number | null; awayScore: number | null;
  venue: string | null; matchLabel: string | null; matchday: number;
}
export interface GroupDto { name: string; teams: TeamDto[]; matches: MatchDto[]; }

// ─── Palpites ──────────────────────────────────────────────────────────
export interface PredictionDto {
  id: string; matchId: number; homeScore: number; awayScore: number;
  points: number; isProcessed: boolean; createdAt: string; updatedAt: string;
}

// ─── Ranking ────────────────────────────────────────────────────────────
export interface RankingEntryDto {
  position: number; userId: string; userName: string; userPhotoUrl: string | null;
  totalPoints: number; exactScores: number; correctOutcomes: number; totalPredictions: number;
}

// ─── Grupos do Bolão ────────────────────────────────────────────────────
export interface BolaoGroupDto {
  id: string; name: string; description: string | null;
  creatorId: string; creatorName: string;
  inviteCode: string; inviteLink: string; whatsAppShareUrl: string;
  memberCount: number; myRole: MemberRole; myStatus: MemberStatus;
  createdAt: string;
}
export interface BolaoGroupMemberDto {
  userId: string; userName: string; userPhotoUrl: string | null;
  role: MemberRole; status: MemberStatus;
  invitedAt: string; joinedAt: string | null;
}
export interface GroupInviteInfoDto {
  groupId: string; groupName: string; description: string | null;
  creatorName: string; memberCount: number;
  isAlreadyMember: boolean; currentStatus: MemberStatus | null;
}

// ─── Enums ──────────────────────────────────────────────────────────────
export enum MatchPhase {
  GroupStage = 1, RoundOf32 = 2, RoundOf16 = 3,
  Quarterfinals = 4, Semifinals = 5, ThirdPlace = 6, Final = 7
}
export enum MatchStatus { Scheduled = 1, InProgress = 2, Finished = 3, Cancelled = 4 }
export enum MemberRole { Admin = 1, Member = 2 }
export enum MemberStatus { Pending = 1, Active = 2, Rejected = 3 }
```

---

## Estrutura de pastas

```
src/
├── api/
│   ├── axios.ts             # instância + interceptor JWT
│   ├── auth.ts
│   ├── groups.ts            # grupos da Copa
│   ├── matches.ts
│   ├── predictions.ts
│   ├── ranking.ts
│   ├── users.ts
│   ├── bolaoGroups.ts       # grupos do bolão (convites)
│   └── notifications.ts
├── components/
│   ├── layout/
│   │   ├── BottomNav.tsx
│   │   ├── Header.tsx
│   │   └── PrivateRoute.tsx
│   ├── match/
│   │   ├── MatchCard.tsx
│   │   ├── PredictionInput.tsx
│   │   └── ScoreBadge.tsx
│   ├── bolaoGroup/
│   │   ├── GroupCard.tsx
│   │   ├── InviteCard.tsx
│   │   └── MemberRow.tsx
│   └── ranking/
│       └── RankingRow.tsx
├── hooks/
│   ├── useAuth.ts
│   ├── usePushNotification.ts
│   └── useCountdown.ts
├── pages/
│   ├── auth/
│   │   ├── LoginPage.tsx
│   │   └── RegisterPage.tsx
│   ├── groups/                  # grupos da Copa
│   │   ├── GroupListPage.tsx
│   │   └── GroupDetailPage.tsx
│   ├── matches/
│   │   └── MatchDetailPage.tsx
│   ├── predictions/
│   │   └── MyPredictionsPage.tsx
│   ├── ranking/
│   │   └── RankingPage.tsx
│   ├── profile/
│   │   └── ProfilePage.tsx
│   ├── bolaoGroups/             # grupos do bolão
│   │   ├── MyBolaoGroupsPage.tsx
│   │   ├── BolaoGroupDetailPage.tsx
│   │   └── JoinGroupPage.tsx    # rota /join/:code
│   └── admin/
│       └── AdminPage.tsx
├── store/
│   └── authStore.ts
├── types/
│   └── api.ts
└── utils/
    ├── cn.ts
    └── formatters.ts
```

---

## Rotas (src/App.tsx)

```tsx
<Routes>
  {/* Públicas */}
  <Route path="/login" element={<LoginPage />} />
  <Route path="/register" element={<RegisterPage />} />

  {/* Página de convite — acessível sem login, mas redireciona para auth se necessário */}
  <Route path="/join/:code" element={<JoinGroupPage />} />

  {/* Privadas */}
  <Route element={<PrivateRoute />}>
    <Route path="/" element={<GroupListPage />} />
    <Route path="/groups/:name" element={<GroupDetailPage />} />
    <Route path="/matches/:id" element={<MatchDetailPage />} />
    <Route path="/predictions" element={<MyPredictionsPage />} />
    <Route path="/ranking" element={<RankingPage />} />
    <Route path="/profile" element={<ProfilePage />} />
    <Route path="/meus-grupos" element={<MyBolaoGroupsPage />} />
    <Route path="/meus-grupos/:id" element={<BolaoGroupDetailPage />} />
    <Route path="/admin" element={<AdminPage />} />
  </Route>
</Routes>
```

---

## Endpoints da API

### Auth
```
POST /api/auth/register      { name, phoneNumber, password }  → AuthResponse
POST /api/auth/login         { phoneNumber, password }        → AuthResponse
POST /api/auth/change-password  { currentPassword, newPassword }  → 204
```

### Usuário
```
GET  /api/users/me                        → UserDto
PUT  /api/users/me        { name, phoneNumber? }  → UserDto
POST /api/users/me/photo  multipart: file         → { photoUrl }
```

### Grupos da Copa e Jogos
```
GET /api/groups               → GroupDto[]
GET /api/groups/:name         → GroupDto
GET /api/matches/:id          → MatchDto
GET /api/matches/upcoming     → MatchDto[]   (?hours=24)
GET /api/matches/phase/:phase → MatchDto[]
```

### Palpites
```
GET  /api/predictions                    → PredictionDto[]
GET  /api/predictions/match/:matchId     → PredictionDto | null
POST /api/predictions  { matchId, homeScore, awayScore }  → PredictionDto
```

### Ranking Global
```
GET /api/ranking      → RankingEntryDto[]
GET /api/ranking/me   → RankingEntryDto
```

### Grupos do Bolão ⭐ (nova feature)
```
POST   /api/bolao-groups                          { name, description? }  → BolaoGroupDto
GET    /api/bolao-groups                          → BolaoGroupDto[]   (meus grupos)
GET    /api/bolao-groups/:id                      → BolaoGroupDto
GET    /api/bolao-groups/invite/:code             → GroupInviteInfoDto  (sem auth)
POST   /api/bolao-groups/invite/:code/accept      → BolaoGroupDto
POST   /api/bolao-groups/invite/:code/reject      → 204
GET    /api/bolao-groups/:id/members              → BolaoGroupMemberDto[]
GET    /api/bolao-groups/:id/ranking              → RankingEntryDto[]
DELETE /api/bolao-groups/:id/members/:userId      → 204  (admin)
POST   /api/bolao-groups/:id/leave                → 204
POST   /api/bolao-groups/:id/regenerate-invite    → { inviteLink }  (admin)
```

### Notificações Push
```
GET    /api/notifications/vapid-public-key  → { publicKey }
POST   /api/notifications/subscribe        { endpoint, p256dh, auth, deviceInfo? }
DELETE /api/notifications/unsubscribe      ?endpoint=...
```

### Admin
```
GET   /api/admin/matches              → MatchDto[]  (?status=)
PATCH /api/admin/matches/:id/result   { homeScore, awayScore }
POST  /api/admin/matches/:id/start
POST  /api/admin/generate-next-phase
POST  /api/admin/send-notification    { title, body }
POST  /api/admin/users/:id/toggle-admin
```

---

## Telas e comportamentos

### JoinGroupPage (`/join/:code`) ⭐
Esta é a tela mais importante do fluxo de convite.

**Fluxo:**
1. Antes de carregar: salvar o `code` no `sessionStorage` (chave: `pendingInviteCode`)
2. Chamar `GET /api/bolao-groups/invite/:code` (sem auth) para exibir preview do grupo
3. Exibir: nome do grupo, descrição, criador, número de membros
4. **Se não autenticado**: mostrar botões "Entrar / Criar conta" e "Já tenho conta — Fazer login"
   - Ambos redirecionam para `/register?redirect=/join/:code` ou `/login?redirect=/join/:code`
5. **Se autenticado + `currentStatus === null`**: mostrar botões "Aceitar convite" e "Recusar"
6. **Se `isAlreadyMember === true`**: mostrar "Você já é membro! Ver grupo →"
7. **Se `currentStatus === Rejected`**: mostrar "Você recusou antes. Aceitar agora?"

**Após login/cadastro:**
- Verificar se há `pendingInviteCode` no `sessionStorage`
- Se sim, redirecionar automaticamente para `/join/:code`
- Limpar o `sessionStorage` após aceitar ou recusar

**Componente de preview do grupo:**
```tsx
<InviteCard>
  <h1>{groupInfo.groupName}</h1>
  <p>Criado por {groupInfo.creatorName}</p>
  <p>{groupInfo.memberCount} participante(s)</p>
  <p>{groupInfo.description}</p>
  <Button onClick={accept}>⚽ Aceitar convite</Button>
  <Button variant="ghost" onClick={reject}>Recusar</Button>
</InviteCard>
```

### RegisterPage / LoginPage — adaptar para `?redirect=`
```tsx
const [searchParams] = useSearchParams();
const redirect = searchParams.get('redirect') ?? '/';
// Após auth bem-sucedida:
navigate(redirect, { replace: true });
```

### MyBolaoGroupsPage (`/meus-grupos`) ⭐
- Lista cards dos grupos onde o usuário é membro ativo
- Cada card: nome, criador, nº de membros, meu papel (Admin/Membro)
- Botão flutuante `+` → modal "Criar novo grupo" (nome + descrição opcional)
- Ao criar: exibir imediatamente o **modal de compartilhamento** com:
  - Link de convite (copiável)
  - Botão "Compartilhar no WhatsApp" usando `whatsAppShareUrl` direto da resposta
  - QR Code opcional (lib `qrcode.react`)

### BolaoGroupDetailPage (`/meus-grupos/:id`) ⭐
Tabs: **Ranking do Grupo** | **Membros** | **Configurações**

**Tab Ranking:** igual ao RankingPage global mas usando `GET /api/bolao-groups/:id/ranking`

**Tab Membros:**
- Lista todos os membros com avatar, nome, papel e status
- Admin vê botão "Remover" (ícone 🗑️) em membros não-admin
- Qualquer membro vê botão "Sair do grupo" no final

**Tab Configurações** (visível apenas para Admin):
- Editar nome e descrição do grupo (PUT — usar endpoint de update quando implementado)
- Botão "Regenerar link de convite" → `POST /api/bolao-groups/:id/regenerate-invite`
  - Confirmar antes ("O link anterior deixará de funcionar")
- Botão "Compartilhar convite" → abre bottom sheet com link e WhatsApp

**Compartilhamento WhatsApp:**
```tsx
// O backend já retorna a URL pronta — apenas abrir:
window.open(group.whatsAppShareUrl, '_blank');
```

### GroupListPage (Home `/`) — adaptar
- Manter os 12 grupos da Copa como antes
- Adicionar seção "Meus Grupos do Bolão" no topo com cards compactos
- Badge com número de grupos ativos

### RankingPage (`/ranking`) — adaptar
- Tabs: **Geral** | **Meu Grupo** (se o usuário estiver em pelo menos 1 grupo)
- Tab "Meu Grupo": dropdown para selecionar qual grupo ver se for membro de múltiplos

---

## Fluxo completo de convite — Diagrama

```
Usuário A cria grupo
       ↓
Recebe whatsAppShareUrl da API
       ↓
Clica "Compartilhar no WhatsApp"
       ↓
WhatsApp abre com mensagem + link pré-preenchidos
       ↓
Usuário B recebe o link e clica
       ↓
Abre /join/:code no browser
       ↓
Vê preview do grupo (nome, criador, membros)
       ↓
[Se não tem conta] → /register → volta para /join/:code
[Se tem conta]     → /login   → volta para /join/:code
       ↓
Aceita ou Rejeita o convite
       ↓
[Aceito] → vai para /meus-grupos/:id com toast "Bem-vindo ao grupo!"
[Rejeitado] → vai para / com toast "Convite recusado"
```

---

## Componente `InviteCard.tsx`

```tsx
interface Props {
  info: GroupInviteInfoDto;
  onAccept: () => void;
  onReject: () => void;
  isLoading: boolean;
}

export function InviteCard({ info, onAccept, onReject, isLoading }: Props) {
  return (
    <div className="min-h-screen flex items-center justify-center bg-green-50 p-4">
      <div className="bg-white rounded-2xl shadow-xl p-8 max-w-sm w-full text-center">
        <div className="text-6xl mb-4">⚽🏆</div>
        <h1 className="text-2xl font-bold text-green-700">{info.groupName}</h1>
        {info.description && <p className="text-gray-500 mt-2">{info.description}</p>}
        <p className="mt-4 text-sm text-gray-400">
          Criado por <strong>{info.creatorName}</strong>
        </p>
        <p className="text-sm text-gray-400">
          {info.memberCount} participante{info.memberCount !== 1 ? 's' : ''}
        </p>

        {info.isAlreadyMember ? (
          <p className="mt-6 text-green-600 font-semibold">Você já é membro deste grupo! ✅</p>
        ) : (
          <div className="mt-6 flex flex-col gap-3">
            <Button onClick={onAccept} disabled={isLoading} className="w-full bg-green-600">
              ⚽ Aceitar convite
            </Button>
            <Button onClick={onReject} disabled={isLoading} variant="ghost" className="w-full">
              Recusar
            </Button>
          </div>
        )}
      </div>
    </div>
  );
}
```

---

## Compartilhamento WhatsApp (GroupCard.tsx)

```tsx
function ShareButton({ group }: { group: BolaoGroupDto }) {
  const [copied, setCopied] = useState(false);

  const copyLink = () => {
    navigator.clipboard.writeText(group.inviteLink);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  return (
    <div className="flex gap-2">
      <Button onClick={copyLink} variant="outline" size="sm">
        {copied ? '✅ Copiado!' : '🔗 Copiar link'}
      </Button>
      <Button
        onClick={() => window.open(group.whatsAppShareUrl, '_blank')}
        className="bg-[#25D366] hover:bg-[#128C7E] text-white"
        size="sm"
      >
        {/* ícone WhatsApp */}
        <svg viewBox="0 0 24 24" className="w-4 h-4 mr-1 fill-current">
          <path d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347z"/>
          <path d="M12 0C5.373 0 0 5.373 0 12c0 2.096.541 4.064 1.488 5.775L0 24l6.432-1.687A11.944 11.944 0 0012 24c6.627 0 12-5.373 12-12S18.627 0 12 0zm0 21.882a9.882 9.882 0 01-5.034-1.376l-.36-.214-3.733.979.996-3.641-.235-.374A9.882 9.882 0 012.118 12c0-5.45 4.432-9.882 9.882-9.882S21.882 6.55 21.882 12c0 5.45-4.432 9.882-9.882 9.882z"/>
        </svg>
        WhatsApp
      </Button>
    </div>
  );
}
```

---

## BottomNav — adicionar item Grupos

```tsx
const navItems = [
  { path: '/',             icon: <Home />,       label: 'Copa'    },
  { path: '/meus-grupos',  icon: <Users />,       label: 'Grupos'  },
  { path: '/ranking',      icon: <Trophy />,      label: 'Ranking' },
  { path: '/predictions',  icon: <Target />,      label: 'Palpites'},
  { path: '/profile',      icon: <User />,        label: 'Perfil'  },
];
```

---

## TanStack Query — Keys de referência

```ts
export const queryKeys = {
  // Copa
  groups: ['groups'] as const,
  group: (name: string) => ['groups', name] as const,
  match: (id: number) => ['matches', id] as const,
  upcoming: (hours: number) => ['matches', 'upcoming', hours] as const,
  predictions: ['predictions'] as const,
  predictionForMatch: (id: number) => ['predictions', 'match', id] as const,
  ranking: ['ranking'] as const,
  myRanking: ['ranking', 'me'] as const,

  // Grupos do Bolão
  bolaoGroups: ['bolao-groups'] as const,
  bolaoGroup: (id: string) => ['bolao-groups', id] as const,
  bolaoGroupInvite: (code: string) => ['bolao-groups', 'invite', code] as const,
  bolaoGroupMembers: (id: string) => ['bolao-groups', id, 'members'] as const,
  bolaoGroupRanking: (id: string) => ['bolao-groups', id, 'ranking'] as const,

  profile: ['users', 'me'] as const,
};
```

---

## Regras de negócio no frontend

1. **Prazo de palpite**: `new Date(match.matchDate) > new Date()` E `match.status === 1`
2. **Fuso horário**: sempre converter UTC → `America/Sao_Paulo` com `date-fns-tz`
3. **Invite flow**: salvar `code` no `sessionStorage` antes de redirecionar para auth
4. **JWT expirado**: interceptor Axios → logout + redirect para `/login`
5. **Múltiplos grupos**: mostrar seletor de grupo no ranking
6. **WhatsApp**: usar `window.open(group.whatsAppShareUrl, '_blank')` — URL já vem pronta da API
7. **Admin do grupo** (MemberRole.Admin = 1): pode remover membros e regenerar convite
8. **Offline**: Workbox `NetworkFirst` para API, grupos em cache de 5 min

---

## PWA Manifest

```ts
manifest: {
  name: 'Bolão Copa 2026',
  short_name: 'Bolão Copa',
  theme_color: '#16a34a',
  background_color: '#ffffff',
  display: 'standalone',
  start_url: '/',
  icons: [
    { src: '/icon-192.png', sizes: '192x192', type: 'image/png' },
    { src: '/icon-512.png', sizes: '512x512', type: 'image/png', purpose: 'any maskable' },
  ],
}
```

---

## Checklist de entregáveis

- [ ] Projeto criado com Vite + React + TypeScript
- [ ] PWA configurada e instalável
- [ ] Service Worker com push notifications
- [ ] Autenticação completa (login, cadastro, logout)
- [ ] `?redirect=` no login/registro para retornar ao convite
- [ ] Persistência do invite code no `sessionStorage`
- [ ] JoinGroupPage com preview, aceitar e recusar
- [ ] MyBolaoGroupsPage com criação de grupo e modal de compartilhamento
- [ ] BolaoGroupDetailPage com ranking do grupo e gestão de membros
- [ ] Botão WhatsApp usando `whatsAppShareUrl` da API
- [ ] Botão copiar link de convite
- [ ] Todos os 12 grupos da Copa com palpites
- [ ] Ranking global + ranking por grupo (com tabs)
- [ ] Upload de foto de perfil
- [ ] Painel admin
- [ ] Responsivo mobile-first (375px → 768px)
- [ ] Build sem erros (`npm run build`)
