# FCG PaymentsAPI

Microsserviço responsável por simular o processamento de pagamentos. Não possui banco de dados nem endpoints HTTP externos - opera exclusivamente via mensageria.

## Responsabilidades

- Consome `OrderPlacedEvent` do RabbitMQ
- Simula aprovação/rejeição do pagamento (configurável via `Payment__RejectProbability`)
- Publica `PaymentProcessedEvent` com status `Approved` ou `Rejected`

## Lógica de Simulação

A probabilidade de rejeição é controlada pela variável `Payment__RejectProbability` (padrão: `0.2` = 20% de chance de rejeição). Para forçar rejeição em testes, use `1.0`.

## Eventos

| Evento | Direção | Fila |
|--------|---------|------|
| `OrderPlacedEvent` | Consome | `payments-order-placed` |
| `PaymentProcessedEvent` | Publica | (exchange MassTransit) |

## Variáveis de Ambiente

| Variável | Descrição | Exemplo |
|----------|-----------|---------|
| `RabbitMQ_Host` | Host do RabbitMQ | `rabbitmq` |
| `RabbitMQ_Username` | Usuário RabbitMQ | `fcg` |
| `RabbitMQ_Password` | Senha RabbitMQ | `fcg@pass` |
| `Payment__RejectProbability` | Probabilidade de rejeição (0.0-1.0) | `0.2` |

## Testes

```bash
dotnet test tests/Payments.UnitTests/