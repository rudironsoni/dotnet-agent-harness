const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

function copyDir(src, dest) {
  fs.mkdirSync(dest, { recursive: true });
  const entries = fs.readdirSync(src, { withFileTypes: true });

  for (const entry of entries) {
    const srcPath = path.join(src, entry.name);
    const destPath = path.join(dest, entry.name);
    if (entry.isDirectory()) {
      copyDir(srcPath, destPath);
    } else {
      fs.copyFileSync(srcPath, destPath);
    }
  }
}

async function buildPlatformPackage(config) {
  const {
    target,
    generatedDir,
    copyPaths = [],
    rootFiles = [],
    packageDir = process.cwd(),
  } = config;

  if (!target) {
    throw new Error('target is required');
  }

  const projectRoot = path.join(packageDir, '..', '..');
  const bundledDir = path.join(packageDir, 'bundled');

  fs.rmSync(bundledDir, { recursive: true, force: true });
  fs.mkdirSync(bundledDir, { recursive: true });

  execSync(`npx rulesync generate --targets ${target} --features "*"`, {
    cwd: projectRoot,
    stdio: 'inherit',
  });

  if (copyPaths.length > 0) {
    for (const relPath of copyPaths) {
      const srcPath = path.join(projectRoot, relPath);
      if (!fs.existsSync(srcPath)) {
        continue;
      }

      const destPath = path.join(bundledDir, relPath);
      const stat = fs.statSync(srcPath);
      if (stat.isDirectory()) {
        copyDir(srcPath, destPath);
      } else {
        fs.mkdirSync(path.dirname(destPath), { recursive: true });
        fs.copyFileSync(srcPath, destPath);
      }
    }
  } else if (generatedDir) {
    const srcDir = path.join(projectRoot, generatedDir);
    if (!fs.existsSync(srcDir)) {
      throw new Error(`Generated directory not found: ${generatedDir}`);
    }
    copyDir(srcDir, path.join(bundledDir, generatedDir));
  }

  for (const rootFile of rootFiles) {
    const srcPath = path.join(projectRoot, rootFile);
    if (!fs.existsSync(srcPath)) {
      continue;
    }
    const destPath = path.join(bundledDir, rootFile);
    fs.mkdirSync(path.dirname(destPath), { recursive: true });
    fs.copyFileSync(srcPath, destPath);
  }
}

module.exports = { buildPlatformPackage };
