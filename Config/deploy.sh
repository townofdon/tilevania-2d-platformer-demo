#!/bin/bash

USERNAME="donjuanjavier"
GAME="rexel-2d"
CHANNEL="html"

#
# COLORS
# see: https://stackoverflow.com/questions/5947742/how-to-change-the-output-color-of-echo-in-linux
#
NC='\033[0m'              # No Color
BLACK='\033[0;30m'        # Black
RED='\033[0;31m'          # Red
GREEN='\033[0;32m'        # Green
YELLOW='\033[0;33m'       # Yellow
BLUE='\033[0;34m'         # Blue
PURPLE='\033[0;35m'       # Purple
CYAN='\033[0;36m'         # Cyan
WHITE='\033[0;37m'        # White
GREY='\033[1;30m'         # Grey

#
# UTILS
#
log() {
  echo -e "${GREY}${1}${NC}"
}
info() {
  echo -e "${CYAN}${1}${NC}"
}
success() {
  echo -e "${GREEN}✓ ${1}${NC}"
}
warn() {
  echo -e "${YELLOW}${1}${NC}"
}
error() {
  echo -e "${RED}${1}${NC}"
}
prompt() {
  read -p "$1 " -n 1 -r
  echo    # (optional) move to a new line
  if [[ ! $REPLY =~ ^[Yy]$ ]]
  then
      warn "user cancelled."
      exit 1
  fi
}
assertFileExists() {
  if [ ! -f "$1" ]; then
      error "$1 does not exist."
      exit 1
  fi
}
assertDirExists() {
  if [ ! -d "$1" ]; then
      error "$1 does not exist."
      exit 1
  fi
}

#
# GET VERSION
#
VERSION=$(cat version.json \
  | grep version \
  | head -1 \
  | awk -F: '{ print $2 }' \
  | sed 's/[",]//g' \
  | tr -d '[[:space:]]')
SAFE_VERSION="${VERSION//./$'-'}"

#
# SCRIPT
#

info "WELCOME TO THE UNITY DEPLOYMENT SCRIPT!"
info "About to push version ${RED}${VERSION}${CYAN} - proceed?"
prompt "(y/n)"

assertFileExists "../Build/index.html"
mkdir -p "../Archives"
ZIPFILE="../Archives/build-${SAFE_VERSION}.zip"

log "creating zip archive for ${ZIPFILE}..."

# zip
7z a $ZIPFILE "../Build" > NUL

log "deploying to itch.io..."

# push to itch.io
butler push $ZIPFILE "${USERNAME}/${GAME}:${CHANNEL}" --userversion $VERSION

success "All done!"
