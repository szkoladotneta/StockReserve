#!/usr/bin/env bash
# Fires N parallel reservation requests for the same product and shows how many succeeded.
# Usage: ./demo/race.sh [productId] [parallelism]
#
# Before you run it: how many 201s do you expect for a product with Available = 1?

set -euo pipefail

HOST="${HOST:-http://localhost:5080}"
PRODUCT="${1:-22222222-2222-2222-2222-222222222222}"   # the limited-edition mouse, OnHand = 1
N="${2:-20}"

echo "Product before:"
curl -s "$HOST/products/$PRODUCT" | python3 -m json.tool
echo
echo "Firing $N parallel reservations..."

seq 1 "$N" | xargs -P "$N" -I{} sh -c \
  "curl -s -o /dev/null -w '%{http_code}\n' -X POST '$HOST/reservations' \
     -H 'Content-Type: application/json' \
     -d '{\"productId\":\"$PRODUCT\",\"quantity\":1,\"customerEmail\":\"client{}@example.com\"}'" \
  | sort | uniq -c | sed 's/^/  /'

echo
echo "Product after:"
curl -s "$HOST/products/$PRODUCT" | python3 -m json.tool
