#!/bin/bash
# SKILLZ-TAP-RUSH Sync Script
# Usage: ./sync.sh pull|push|status

cd /opt/SKILLZ-TAP-RUSH

case " in
  pull)
    echo [Sync] Pulling from GitHub...
    git pull origin main --ff-only
    echo [Sync] Pull complete
    ;;
  push)
    echo [Sync] Pushing to GitHub...
    git push origin main
    echo [Sync] Push complete
    ;;
  status)
    echo [Sync] Current status:
    git status
    echo "
    echo [Sync] Remote:
    git remote -v
    ;;
  *)
    echo Usage: {pull|push|status}
    exit 1
    ;;
esac
