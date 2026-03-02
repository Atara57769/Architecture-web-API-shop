## Plan for Decomposing WebApiShop Monolith into Microservices

**TL;DR**  
The existing ASP.NET Core monolith contains users, products, categories, orders, ratings and password logic. We’ll carve it into a set of bounded‑context services (User/Auth, Product/Catalog, Order, Rating, …), choose technology stacks per service (keep .NET where it makes sense but allow polyglot), and pick appropriate storage (relational for transactions, NoSQL/event store where eventual consistency is fine). The plan explains domains, service boundaries, data ownership, communication patterns and migration steps.

---

### 1. **Domain Decomposition**

1. **User/Authentication Service** (`UserSvc`)  
   - Manages user profiles, registration, login, roles.  
   - Holds `Users`, `Passwords` tables and zxcvbn validation.  
   - Issue JWTs or integrate with OAuth2.  
   - Language: C#/.NET (reuses existing code); could be Node/Go if team prefers polyglot.  
   - DB: Relational (SQL Server or PostgreSQL) for strong consistency + ACID for credentials.

2. **Catalog Service**  
   - Products + Categories.  
   - Exposes product search, filtering, category hierarchy.  
   - Language: C#/.NET initially; can evolve independently.  
   - DB: Relational or document DB (e.g. PostgreSQL with JSONB, MongoDB) depending on query needs.

3. **Order Service**  
   - Handles orders, order items, status, inventory reservation if added later.  
   - Owns `Orders`/`OrderItems` entities and orchestrates fulfilment.  
   - Language: C# or JVM/Go for performance.  
   - DB: Relational for transactional integrity; consider separate SQL instance.

4. **Rating Service**  
   - Manages product ratings & reviews.  
   - Can be eventual‑consistent; denormalized read models.  
   - Language: lightweight (Go, Node, .NET).  
   - DB: Document store (MongoDB/Cosmos) or relational with simpler schema.

5. **Category Service** (optional split)  
   - If category tree becomes complex, treat separately; otherwise embed in Catalog.

6. **Gateway/API Aggregator**  
   - Front door for clients; routes requests to services, handles auth, rate‑limit.  
   - Could be Kong, Ocelot (.NET) or NGINX with Lua.

7. **Common/Shared Libraries**  
   - DTOs, logging, error middleware can be packaged as NuGet or kept in separate repos for reuse.

---

### 2. **Technology & Storage Recommendations**

- **Languages**  
  - **Primary**: Continue with C#/.NET to leverage existing codebase, teams, and integrations.  
  - **Polyglot Option**: Allow teams to pick Go, Node.js, Java, Python for new services if justified by domain requirements (e.g. high‑performance order engine in Go).  
  - **Rationale**: microservices enable technology experimentation; start with .NET for minimal disruption.

- **Databases**  
  - **Relational (SQL Server, PostgreSQL)** for user credentials, orders, transactional consistency.  
  - **Document/NoSQL (MongoDB, Cosmos DB, DynamoDB)** for ratings, catalog metadata where flexible schema helps.  
  - **Event Store or Kafka** for cross‑service events (order placed, product updated).  
  - **Per‑service DB ownership**: no shared database; each service owns its schema to avoid coupling.

---

### 3. **Communication Patterns**

1. **Synchronous HTTP/REST** between gateway and services.  
2. **Asynchronous messaging** (RabbitMQ/Kafka) for events: `OrderCreated`, `ProductUpdated`, `UserRegistered`.  
3. **API contracts**: use OpenAPI specs; each service publishes its own swagger.

---

### 4. **Migration Strategy**

1. **Extract services one at a time**  
   - Begin with low‑risk domain (Rating or Catalog).  
   - Create new repos/projects with existing controllers/services mapped.  
   - Deploy side‑by‑side with monolith; modify gateway to route.

2. **Data replication**  
   - For initial reads, replicate data from monolith DB to service DB via change data capture or ETL.  
   - Stop updating monolith for that domain once service is live.

3. **Strangling the monolith**  
   - Gradually move controllers and business logic into services.  
   - Replace internal calls with HTTP/message calls.

4. **Testing & Validation**  
   - Maintain existing unit/integration tests per service.  
   - Add contract tests (Pact) to ensure backward compatibility.

5. **Deployment**  
   - Containerize each service (Docker).  
   - Use orchestration (Kubernetes, Docker Compose) for local dev.  
   - CI/CD pipelines per service.

6. **Operational concerns**  
   - Centralized logging (ELK/Seq) and monitoring (Prometheus/Grafana).  
   - Circuit breakers, retries (Polly).  
   - Version APIs carefully; deprecate monolith endpoints.

---

### 5. **Verification**

- **Manual Checks**  
  - Hit gateway endpoints and confirm correct service responses.  
  - Ensure user registration/login works after migration.
- **Automated Tests**  
  - Run service-specific unit tests with `dotnet test`.  
  - Execute integration tests targeting individual service databases and through gateway.  
- **Load Testing**  
  - Benchmark services independently to validate DB choices.

---

### 6. **Decisions**

- Start with all services in C#/.NET to leverage code reuse.  
- Use SQL Server for transactional services; consider PostgreSQL for new deploys to avoid vendor lock‑in.  
- Allow eventual polyglot expansion after first two services are stable.  
- Adopt event‑driven communication for decoupling and scalability.

---

This plan gives the technical roadmap for decomposing the monolith, choosing language/DB per domain, and migrating incrementally while maintaining functionality.
