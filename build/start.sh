#!/usr/bin/env bash

set -e

DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PID="$DIR/web/nginx.pid"
NGINX_CONF="$DIR/web/nginx.conf"
NGROK_PID="$DIR/ngrok.pid"

start() {
    if [ -f "$PID" ] && kill -0 "$(cat "$PID")" 2>/dev/null; then
        echo "Nginx is already running (PID $(cat "$PID"))"
    else
        echo "Starting Nginx..."
        nginx -c "$NGINX_CONF" -p "$DIR/web/"
        echo "Nginx started on http://localhost:8081"
    fi
}

stop() {
    if [ -f "$PID" ] && kill -0 "$(cat "$PID")" 2>/dev/null; then
        echo "Stopping Nginx..."
        nginx -s stop -c "$NGINX_CONF" -p "$DIR/web/"
        echo "Nginx stopped"
    else
        echo "Nginx is not running"
    fi
}

restart() {
    stop
    sleep 1
    start
}

status() {
    if [ -f "$PID" ] && kill -0 "$(cat "$PID")" 2>/dev/null; then
        echo "Nginx: RUNNING (PID $(cat "$PID"))"
        echo "URL:   http://localhost:8081"
    else
        echo "Nginx: STOPPED"
    fi

    if [ -f "$NGROK_PID" ] && kill -0 "$(cat "$NGROK_PID")" 2>/dev/null; then
        echo "ngrok: RUNNING (PID $(cat "$NGROK_PID"))"
    else
        echo "ngrok: STOPPED"
    fi
}

ngrok_start() {
    if [ -f "$NGROK_PID" ] && kill -0 "$(cat "$NGROK_PID")" 2>/dev/null; then
        echo "ngrok is already running"
        return
    fi

    if ! command -v ngrok >/dev/null 2>&1; then
        echo "Error: ngrok is not installed"
        exit 1
    fi

    echo "Starting ngrok..."
    ngrok http 8081 > "$DIR/ngrok.log" 2>&1 &
    echo $! > "$NGROK_PID"

    sleep 2
    echo "ngrok started"
    echo "Check $DIR/ngrok.log for the public URL"
}

ngrok_stop() {
    if [ -f "$NGROK_PID" ] && kill -0 "$(cat "$NGROK_PID")" 2>/dev/null; then
        echo "Stopping ngrok..."
        kill "$(cat "$NGROK_PID")"
        rm -f "$NGROK_PID"
        echo "ngrok stopped"
    else
        echo "ngrok is not running"
    fi
}

case "${1:-}" in
    start)
        start
        ;;
    stop)
        stop
        ;;
    restart)
        restart
        ;;
    status)
        status
        ;;
    ngrok-start)
        ngrok_start
        ;;
    ngrok-stop)
        ngrok_stop
        ;;
    up)
        start
        ngrok_start
        ;;
    down)
        ngrok_stop
        stop
        ;;
    *)
        echo "Usage: $0 {start|stop|restart|status|ngrok-start|ngrok-stop|up|down}"
        echo
        echo "  start        Start Nginx"
        echo "  stop         Stop Nginx"
        echo "  restart      Restart Nginx"
        echo "  status       Show Nginx/ngrok status"
        echo "  ngrok-start  Start ngrok"
        echo "  ngrok-stop   Stop ngrok"
        echo "  up           Start Nginx + ngrok"
        echo "  down         Stop ngrok + Nginx"
        exit 1
        ;;
esac
