#!/usr/bin/env bash
set -euo pipefail

COMPOSE_FILE="docker-compose.sandbox.yml"

usage() {
    echo "Usage: $0 {up|down|logs|rebuild|reset}"
    echo
    echo "  up       Start sandbox environment"
    echo "  down     Stop sandbox environment (keeps data)"
    echo "  logs     View sandbox logs"
    echo "  rebuild  Rebuild and restart sandbox"
    echo "  reset    Destroy all data and restart from scratch"
    exit 1
}

[[ $# -eq 0 ]] && usage

case "$1" in
    up)
        docker compose -f "$COMPOSE_FILE" up -d
        ;;
    down)
        docker compose -f "$COMPOSE_FILE" down
        ;;
    logs)
        docker compose -f "$COMPOSE_FILE" logs -f
        ;;
    rebuild)
        docker compose -f "$COMPOSE_FILE" up -d --build
        ;;
    reset)
        docker compose -f "$COMPOSE_FILE" down -v
        docker compose -f "$COMPOSE_FILE" up -d
        ;;
    *)
        usage
        ;;
esac
