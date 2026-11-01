import { Section } from "./section";
import { ServerSummary } from "./server";
import { User } from "./user";

export interface HomeDto{
    servers: ServerSummary[] | null;
    friends: User[] | null;
}


export interface HomeView {
  servers: Section<ServerSummary[]>;
  friends: Section<User[]>;
} 