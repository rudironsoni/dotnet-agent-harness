const fs = require('fs');
const fsp = require('fs/promises');
const path = require('path');

/**
 * dotnet-agent-harness OpenCode Plugin
 * 
 * This plugin bundles agents, skills, commands, and rules for .NET development
 * and installs them into the project's .opencode/ directory.
 *
 * Bundled directory layout (mirrors OpenCode's generated structure):
 *   bundled/agent/      -> .opencode/agent/
 *   bundled/command/    -> .opencode/command/
 *   bundled/skill/      -> .opencode/skill/
 *   bundled/memories/   -> .opencode/memories/
 *   bundled/plugins/    -> .opencode/plugins/
 */

const PLUGIN_NAME = 'dotnet-agent-harness';

// Read version from package.json at load time (sync is acceptable here, runs once)
const PLUGIN_VERSION = (() => {
  try {
    const pkg = JSON.parse(fs.readFileSync(path.join(__dirname, 'package.json'), 'utf8'));
    return pkg.version || '0.0.0';
  } catch {
    return '0.0.0';
  }
})();

// Content directories to install (same names in bundled/ and .opencode/)
const CONTENT_DIRS = ['agent', 'command', 'skill', 'memories', 'plugins'];

/**
 * Get the bundled content directory
 */
function getBundledDir() {
  return path.join(__dirname, 'bundled');
}

/**
 * Copy directory contents recursively using async I/O
 */
async function copyDir(src, dest) {
  await fsp.mkdir(dest, { recursive: true });

  const entries = await fsp.readdir(src, { withFileTypes: true });

  for (const entry of entries) {
    const srcPath = path.join(src, entry.name);
    const destPath = path.join(dest, entry.name);

    if (entry.isDirectory()) {
      await copyDir(srcPath, destPath);
    } else {
      await fsp.copyFile(srcPath, destPath);
    }
  }
}

/**
 * Remove directory contents recursively for plugin uninstall
 */
async function removeInstalledContent(projectDir) {
  const opencodeDir = path.join(projectDir, '.opencode');

  for (const dirName of CONTENT_DIRS) {
    const destDir = path.join(opencodeDir, dirName);
    try {
      await fsp.rm(destDir, { recursive: true, force: true });
      console.log(`[${PLUGIN_NAME}] Removed ${dirName}/ from ${destDir}`);
    } catch {
      // Directory may not exist; that's fine
    }
  }
}

/**
 * Install bundled content to project
 */
async function installBundledContent(projectDir) {
  const bundledDir = getBundledDir();

  try {
    await fsp.access(bundledDir);
  } catch {
    console.warn(`[${PLUGIN_NAME}] Warning: bundled directory not found at ${bundledDir}. Run 'npm run build' first.`);
    return;
  }

  const opencodeDir = path.join(projectDir, '.opencode');

  for (const dirName of CONTENT_DIRS) {
    const srcDir = path.join(bundledDir, dirName);
    try {
      await fsp.access(srcDir);
      const destDir = path.join(opencodeDir, dirName);
      await copyDir(srcDir, destDir);
      console.log(`[${PLUGIN_NAME}] Installed ${dirName}/ to ${destDir}`);
    } catch {
      // Source directory may not exist for all content types; skip silently
    }
  }
}

/**
 * Main plugin export
 * 
 * @param {Object} context - OpenCode plugin context
 * @param {Object} [context.project] - Project information
 * @param {Object} [context.client] - OpenCode client
 * @param {Object} [context.$] - Shell utilities
 * @param {string} [context.directory] - Project directory
 * @param {string} [context.worktree] - Git worktree
 * @returns {Object} Plugin hooks
 */
module.exports = function dotnetAgentHarnessPlugin(context) {
  if (!context || typeof context !== 'object') {
    console.error(`[${PLUGIN_NAME}] Invalid plugin context provided`);
    return {};
  }

  const directory = context.directory || process.cwd();

  console.log(`[${PLUGIN_NAME}] Plugin loaded (v${PLUGIN_VERSION})`);

  return {
    // Install bundled content when plugin is initialized
    'plugin.install': async () => {
      console.log(`[${PLUGIN_NAME}] Installing bundled content...`);
      await installBundledContent(directory);
      console.log(`[${PLUGIN_NAME}] Installation complete`);
    },

    // Clean up installed content when plugin is uninstalled
    'plugin.uninstall': async () => {
      console.log(`[${PLUGIN_NAME}] Uninstalling bundled content...`);
      await removeInstalledContent(directory);
      console.log(`[${PLUGIN_NAME}] Uninstall complete`);
    },

    // Re-install on project updates
    'installation.updated': async () => {
      console.log(`[${PLUGIN_NAME}] Re-installing bundled content...`);
      await installBundledContent(directory);
    },

    // Provide info about the plugin
    'plugin.info': () => ({
      name: PLUGIN_NAME,
      version: PLUGIN_VERSION,
      description: '.NET Agent Harness - 14 specialist agents, 131 skills, commands, and rules',
      agents: 14,
      skills: 131
    })
  };
};
