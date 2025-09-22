#!/bin/sh
set -e
ollama serve &
sleep 5
ollama pull gemma2:2b
wait
