#!/bin/sh
# ===========================================
# FULL E2E TEST - Expense Tracker
# Tests all endpoints from api.http
# ===========================================
set -e

AUTH="http://localhost:5100"
PERM="http://localhost:5200"
EXPENSE="http://localhost:5000"

pass=0
fail=0

check() {
    local desc="$1" expected="$2" actual="$3" body="$4"
    if [ "$actual" = "$expected" ]; then
        pass=$((pass+1))
        echo "  ✅ $desc (HTTP $actual)"
    else
        fail=$((fail+1))
        echo "  ❌ $desc (expected $expected, got $actual)"
        if [ -n "$body" ]; then
            echo "     Body: $body"
        fi
    fi
}

echo "==========================================="
echo " AUTH SERVICE - Register"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" -X POST "$AUTH/auth/register" \
  -H "Content-Type: application/json" \
  -d '{"email":"john.doe@user.com","firstName":"John","lastName":"Doe","password":"Str0ngP@ss!"}')
HTTP_CODE=$(echo "$RESP" | tail -1)
BODY=$(echo "$RESP" | sed '$d')
USER_ID=$(echo "$BODY" | sed 's/^"//;s/"$//')
check "Register User" "201" "$HTTP_CODE" "$BODY"

echo ""
echo "==========================================="
echo " AUTH SERVICE - Login (Standard user)"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" -X POST "$AUTH/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"john.doe@user.com","password":"Str0ngP@ss!"}')
HTTP_CODE=$(echo "$RESP" | tail -1)
BODY=$(echo "$RESP" | sed '$d')
TOKEN=$(echo "$BODY" | sed 's/.*"token":"\([^"]*\)".*/\1/')
check "Login (Standard)" "200" "$HTTP_CODE" "$BODY"

echo ""
echo "==========================================="
echo " AUTH SERVICE - Get User by ID"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" "$AUTH/auth/users/$USER_ID" \
  -H "Authorization: Bearer $TOKEN")
HTTP_CODE=$(echo "$RESP" | tail -1)
BODY=$(echo "$RESP" | sed '$d')
check "Get User by ID" "200" "$HTTP_CODE" "$BODY"

echo ""
echo "==========================================="
echo " AUTH SERVICE - Assign Admin Role"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" -X POST "$AUTH/auth/users/$USER_ID/roles/11111111-1111-1111-1111-111111111111" \
  -H "Authorization: Bearer $TOKEN")
HTTP_CODE=$(echo "$RESP" | tail -1)
check "Assign Admin Role" "200" "$HTTP_CODE"

echo ""
echo "==========================================="
echo " AUTH SERVICE - Login again (Admin user)"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" -X POST "$AUTH/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"john.doe@user.com","password":"Str0ngP@ss!"}')
HTTP_CODE=$(echo "$RESP" | tail -1)
BODY=$(echo "$RESP" | sed '$d')
TOKEN=$(echo "$BODY" | sed 's/.*"token":"\([^"]*\)".*/\1/')
check "Login (Admin)" "200" "$HTTP_CODE" "$BODY"

echo ""
echo "==========================================="
echo " AUTH SERVICE - Remove Admin Role"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" -X DELETE "$AUTH/auth/users/$USER_ID/roles/11111111-1111-1111-1111-111111111111" \
  -H "Authorization: Bearer $TOKEN")
HTTP_CODE=$(echo "$RESP" | tail -1)
check "Remove Admin Role" "200" "$HTTP_CODE"

echo ""
echo "==========================================="
echo " AUTH SERVICE - Login again (Standard only, after remove)"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" -X POST "$AUTH/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"john.doe@user.com","password":"Str0ngP@ss!"}')
HTTP_CODE=$(echo "$RESP" | tail -1)
BODY=$(echo "$RESP" | sed '$d')
TOKEN=$(echo "$BODY" | sed 's/.*"token":"\([^"]*\)".*/\1/')
check "Login (Standard after remove)" "200" "$HTTP_CODE" "$BODY"

echo ""
echo "==========================================="
echo " AUTH SERVICE - Assign Admin Role back for full test"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" -X POST "$AUTH/auth/users/$USER_ID/roles/11111111-1111-1111-1111-111111111111" \
  -H "Authorization: Bearer $TOKEN")
HTTP_CODE=$(echo "$RESP" | tail -1)
check "Assign Admin Role back" "200" "$HTTP_CODE"

echo ""
echo "==========================================="
echo " AUTH SERVICE - Login again (Admin for rest of tests)"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" -X POST "$AUTH/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"john.doe@user.com","password":"Str0ngP@ss!"}')
HTTP_CODE=$(echo "$RESP" | tail -1)
BODY=$(echo "$RESP" | sed '$d')
TOKEN=$(echo "$BODY" | sed 's/.*"token":"\([^"]*\)".*/\1/')
check "Login (Admin final)" "200" "$HTTP_CODE" "$BODY"

echo ""
echo "==========================================="
echo " AUTH SERVICE - JWKS Public Key"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" "$AUTH/.well-known/jwks")
HTTP_CODE=$(echo "$RESP" | tail -1)
check "JWKS Endpoint" "200" "$HTTP_CODE"

echo ""
echo "==========================================="
echo " PERMISSION SERVICE - Create Role"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" -X POST "$PERM/permissions/roles" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"name":"Manager","permissions":["expenses:create","expenses:read","expenses:update","expenses:delete","categories:create","categories:read","categories:update","categories:delete","payment-methods:read","tags:read","users:access"]}')
HTTP_CODE=$(echo "$RESP" | tail -1)
BODY=$(echo "$RESP" | sed '$d')
ROLE_ID=$(echo "$BODY" | sed 's/^"//;s/"$//')
check "Create Role" "201" "$HTTP_CODE" "$BODY"

echo ""
echo "==========================================="
echo " PERMISSION SERVICE - List Roles"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" "$PERM/permissions/roles" \
  -H "Authorization: Bearer $TOKEN")
HTTP_CODE=$(echo "$RESP" | tail -1)
check "List Roles" "200" "$HTTP_CODE"

echo ""
echo "==========================================="
echo " PERMISSION SERVICE - Get Role by ID"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" "$PERM/permissions/roles/$ROLE_ID" \
  -H "Authorization: Bearer $TOKEN")
HTTP_CODE=$(echo "$RESP" | tail -1)
check "Get Role by ID" "200" "$HTTP_CODE"

echo ""
echo "==========================================="
echo " PERMISSION SERVICE - Update Role Permissions"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" -X PUT "$PERM/permissions/roles/$ROLE_ID/permissions" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '["expenses:read","categories:read","payment-methods:read","tags:read"]')
HTTP_CODE=$(echo "$RESP" | tail -1)
check "Update Role Permissions" "200" "$HTTP_CODE"

echo ""
echo "==========================================="
echo " PERMISSION SERVICE - Delete Role"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" -X DELETE "$PERM/permissions/roles/$ROLE_ID" \
  -H "Authorization: Bearer $TOKEN")
HTTP_CODE=$(echo "$RESP" | tail -1)
check "Delete Role" "204" "$HTTP_CODE"

echo ""
echo "==========================================="
echo " PERMISSION SERVICE - Resolve Permissions"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" -X POST "$PERM/permissions/resolve" \
  -H "Content-Type: application/json" \
  -d '{"roles":["Standard"]}')
HTTP_CODE=$(echo "$RESP" | tail -1)
check "Resolve Permissions" "200" "$HTTP_CODE"

echo ""
echo "==========================================="
echo " EXPENSE SERVICE - Health Check"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" "$EXPENSE/health")
HTTP_CODE=$(echo "$RESP" | tail -1)
check "Health Check" "200" "$HTTP_CODE"

echo ""
echo "==========================================="
echo " EXPENSE SERVICE - Create Category"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" -X POST "$EXPENSE/categories" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"name":"Test Category"}')
HTTP_CODE=$(echo "$RESP" | tail -1)
BODY=$(echo "$RESP" | sed '$d')
CAT_ID=$(echo "$BODY" | sed 's/^"//;s/"$//')
check "Create Category" "200" "$HTTP_CODE" "$BODY"

echo ""
echo "==========================================="
echo " EXPENSE SERVICE - List Categories"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" "$EXPENSE/categories" \
  -H "Authorization: Bearer $TOKEN")
HTTP_CODE=$(echo "$RESP" | tail -1)
check "List Categories" "200" "$HTTP_CODE"

echo ""
echo "==========================================="
echo " EXPENSE SERVICE - Update Category"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" -X PUT "$EXPENSE/categories/$CAT_ID" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"name":"Updated Category"}')
HTTP_CODE=$(echo "$RESP" | tail -1)
check "Update Category" "204" "$HTTP_CODE"

echo ""
echo "==========================================="
echo " EXPENSE SERVICE - Delete Category"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" -X DELETE "$EXPENSE/categories/$CAT_ID" \
  -H "Authorization: Bearer $TOKEN")
HTTP_CODE=$(echo "$RESP" | tail -1)
check "Delete Category" "204" "$HTTP_CODE"

echo ""
echo "==========================================="
echo " EXPENSE SERVICE - Create Payment Method"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" -X POST "$EXPENSE/payment-methods" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"name":"Cartão de Crédito"}')
HTTP_CODE=$(echo "$RESP" | tail -1)
BODY=$(echo "$RESP" | sed '$d')
PM_ID=$(echo "$BODY" | sed 's/^"//;s/"$//')
check "Create Payment Method" "200" "$HTTP_CODE" "$BODY"

echo ""
echo "==========================================="
echo " EXPENSE SERVICE - List Payment Methods"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" "$EXPENSE/payment-methods" \
  -H "Authorization: Bearer $TOKEN")
HTTP_CODE=$(echo "$RESP" | tail -1)
check "List Payment Methods" "200" "$HTTP_CODE"

echo ""
echo "==========================================="
echo " EXPENSE SERVICE - Update Payment Method"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" -X PUT "$EXPENSE/payment-methods/$PM_ID" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"name":"Cartão de Crédito Visa"}')
HTTP_CODE=$(echo "$RESP" | tail -1)
check "Update Payment Method" "204" "$HTTP_CODE"

echo ""
echo "==========================================="
echo " EXPENSE SERVICE - Create Tag"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" -X POST "$EXPENSE/tags" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"name":"urgente"}')
HTTP_CODE=$(echo "$RESP" | tail -1)
BODY=$(echo "$RESP" | sed '$d')
TAG_ID=$(echo "$BODY" | sed 's/^"//;s/"$//')
check "Create Tag" "200" "$HTTP_CODE" "$BODY"

echo ""
echo "==========================================="
echo " EXPENSE SERVICE - List Tags"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" "$EXPENSE/tags" \
  -H "Authorization: Bearer $TOKEN")
HTTP_CODE=$(echo "$RESP" | tail -1)
check "List Tags" "200" "$HTTP_CODE"

echo ""
echo "==========================================="
echo " EXPENSE SERVICE - Update Tag"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" -X PUT "$EXPENSE/tags/$TAG_ID" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"name":"prioridade"}')
HTTP_CODE=$(echo "$RESP" | tail -1)
check "Update Tag" "204" "$HTTP_CODE"

echo ""
echo "==========================================="
echo " EXPENSE SERVICE - Create Expense"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" -X POST "$EXPENSE/expenses" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{\"description\":\"Supermercado\",\"amount\":150.50,\"date\":\"2026-07-05\",\"categoryId\":\"33333333-3333-3333-3333-333333333333\",\"paymentMethodId\":\"$PM_ID\",\"tagIds\":[\"$TAG_ID\"]}")
HTTP_CODE=$(echo "$RESP" | tail -1)
BODY=$(echo "$RESP" | sed '$d')
EXPENSE_ID=$(echo "$BODY" | sed 's/^"//;s/"$//')
check "Create Expense" "200" "$HTTP_CODE" "$BODY"

echo ""
echo "==========================================="
echo " EXPENSE SERVICE - List Expenses"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" "$EXPENSE/expenses" \
  -H "Authorization: Bearer $TOKEN")
HTTP_CODE=$(echo "$RESP" | tail -1)
check "List Expenses" "200" "$HTTP_CODE"

echo ""
echo "==========================================="
echo " EXPENSE SERVICE - List Expenses by User"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" "$EXPENSE/expenses?userId=$USER_ID" \
  -H "Authorization: Bearer $TOKEN")
HTTP_CODE=$(echo "$RESP" | tail -1)
check "List Expenses by User" "200" "$HTTP_CODE"

echo ""
echo "==========================================="
echo " EXPENSE SERVICE - List Expenses by Date Range"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" "$EXPENSE/expenses?from=2026-01-01&to=2026-12-31" \
  -H "Authorization: Bearer $TOKEN")
HTTP_CODE=$(echo "$RESP" | tail -1)
check "List Expenses by Date Range" "200" "$HTTP_CODE"

echo ""
echo "==========================================="
echo " EXPENSE SERVICE - List Expenses by Category"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" "$EXPENSE/expenses?categoryId=33333333-3333-3333-3333-333333333333" \
  -H "Authorization: Bearer $TOKEN")
HTTP_CODE=$(echo "$RESP" | tail -1)
check "List Expenses by Category" "200" "$HTTP_CODE"

echo ""
echo "==========================================="
echo " EXPENSE SERVICE - Get Expense by ID"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" "$EXPENSE/expenses/$EXPENSE_ID" \
  -H "Authorization: Bearer $TOKEN")
HTTP_CODE=$(echo "$RESP" | tail -1)
check "Get Expense by ID" "200" "$HTTP_CODE"

echo ""
echo "==========================================="
echo " EXPENSE SERVICE - Update Expense"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" -X PUT "$EXPENSE/expenses/$EXPENSE_ID" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{\"description\":\"Supermercado - Atualizado\",\"amount\":200.00,\"date\":\"2026-07-05\",\"categoryId\":\"33333333-3333-3333-3333-333333333333\",\"paymentMethodId\":\"$PM_ID\",\"tagIds\":[\"$TAG_ID\"]}")
HTTP_CODE=$(echo "$RESP" | tail -1)
check "Update Expense" "204" "$HTTP_CODE"

echo ""
echo "==========================================="
echo " EXPENSE SERVICE - Delete Expense"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" -X DELETE "$EXPENSE/expenses/$EXPENSE_ID" \
  -H "Authorization: Bearer $TOKEN")
HTTP_CODE=$(echo "$RESP" | tail -1)
check "Delete Expense" "204" "$HTTP_CODE"

echo ""
echo "==========================================="
echo " EXPENSE SERVICE - Delete Payment Method"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" -X DELETE "$EXPENSE/payment-methods/$PM_ID" \
  -H "Authorization: Bearer $TOKEN")
HTTP_CODE=$(echo "$RESP" | tail -1)
check "Delete Payment Method" "204" "$HTTP_CODE"

echo ""
echo "==========================================="
echo " EXPENSE SERVICE - Delete Tag"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" -X DELETE "$EXPENSE/tags/$TAG_ID" \
  -H "Authorization: Bearer $TOKEN")
HTTP_CODE=$(echo "$RESP" | tail -1)
check "Delete Tag" "204" "$HTTP_CODE"

echo ""
echo "==========================================="
echo " ERROR CASES - Expense Not Found"
echo "==========================================="
echo ""

RESP=$(curl -s -w "\n%{http_code}" "$EXPENSE/expenses/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa" \
  -H "Authorization: Bearer $TOKEN")
HTTP_CODE=$(echo "$RESP" | tail -1)
check "Expense Not Found (404)" "404" "$HTTP_CODE"

echo ""
echo "==========================================="
echo " RESULTS"
echo "==========================================="
echo ""
echo "  ✅ Passed: $pass"
echo "  ❌ Failed: $fail"
echo ""

if [ "$fail" -gt 0 ]; then
    exit 1
fi
