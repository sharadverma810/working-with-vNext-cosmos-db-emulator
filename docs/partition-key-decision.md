# Partition Key Decision

## Selected Path

Use `/category` for the sample `Products` container.

## Why This Fits This Learning Project

The dataset will contain several product categories with thousands of
documents distributed across those categories. This makes the partition key
visible in every document and lets the project demonstrate both:

- A category-filtered query that can target one logical partition.
- Broader date, price, status, nested-object, and array queries that can span
  partitions.

Point reads will require both `id` and `category`, which makes the Cosmos DB
point-read contract explicit.

## Alternatives Considered

| Candidate | Decision | Reason |
| --- | --- | --- |
| `/category` | Selected | Supports the primary catalog learning scenario and is easy to inspect. |
| `/status` | Rejected | Too few values and poor distribution; likely to create hot logical partitions. |
| `/warehouseLocation/country` | Rejected | Low cardinality and uneven distribution; not the dominant query pattern. |
| `/id` | Rejected | High cardinality, but category queries would always be cross-partition and the point-read lesson would be less instructive. |
| Synthetic category key | Deferred | Adds modeling complexity without enough value for this 5,000-document local sample. |

## Limitation

`/category` is an educational choice, not a universal production recommendation.
Its low cardinality would require reconsideration for a much larger or
write-heavy catalog. The project will keep query scope and partition behavior
visible so the tradeoff is part of the lesson.