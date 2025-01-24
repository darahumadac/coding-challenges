#!/bin/bash

# Setup hooks
HOOKS_DIR="./scripts/hooks"
echo "[I] Setting git repository config ..."
echo "[I] Setting up repository hooks ..."
git config --local core.hooksPath "${HOOKS_DIR}" && echo -e "[I] Done - Current config: core.hooksPath=$(git config get core.hooksPath)"

