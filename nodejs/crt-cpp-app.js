#!/usr/bin/env node
"use strict";

const fs = require("node:fs");
const path = require("node:path");
const { spawnSync } = require("node:child_process");

const executableName = process.platform === "win32" ? "crt-cpp-app.exe" : "crt-cpp-app";
const executable = path.join(__dirname, "..", "app", executableName);

if (!fs.existsSync(executable)) {
  console.error("crt-cpp-app: bundled application is missing; reinstall this package.");
  process.exit(1);
}

if (process.platform !== "win32") {
  fs.chmodSync(executable, 0o755);
}

const result = spawnSync(executable, process.argv.slice(2), { stdio: "inherit" });
if (result.error) {
  console.error(`crt-cpp-app: ${result.error.message}`);
  process.exit(1);
}

process.exit(result.status ?? 1);
