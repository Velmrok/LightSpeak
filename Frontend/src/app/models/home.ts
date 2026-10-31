
interface HomeDto{
    servers: ServerSummary[] | null;
    friends: User[] | null;
}


interface HomeView {
  servers: Section<ServerSummary[]>;
  friends: Section<User[]>;
}