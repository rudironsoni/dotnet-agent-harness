const fs = require('fs');
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

// Content directories to install (same names in bundled/ and .opencode/)
const CONTENT_DIRS = ['agent', 'command', 'skill', 'memories', 'plugins'];

/**
 * Get the bundled content directory
 */
function getBundledDir() {
  return path.join(__dirname, 'bundled');
}

/**
 * Ensure directory exists
 */
function ensureDir(dir) {
  if (!fs.existsSync(dir)) {
    fs.mkdirSync(dir, { recursive: true });
  }
}

/**
 * Copy directory contents recursively
 */
function copyDir(src, dest) {
  ensureDir(dest);
  
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

/**
 * Install bundled content to project
 */
async function installBundledContent(projectDir) {
  const bundledDir = getBundledDir();
  const opencodeDir = path.join(projectDir, '.opencode');
  
  for (const dirName of CONTENT_DIRS) {
    const srcDir = path.join(bundledDir, dirName);
    if (fs.existsSync(srcDir)) {
      const destDir = path.join(opencodeDir, dirName);
      copyDir(srcDir, destDir);
      console.log(`[${PLUGIN_NAME}] Installed ${dirName}/ to ${destDir}`);
    }
  }
}

/**
 * Main plugin export
 * 
 * @param {Object} context - OpenCode plugin context
 * @param {Object} context.project - Project information
 * @param {Object} context.client - OpenCode client
 * @param {Object} context.$ - Shell utilities
 * @param {string} context.directory - Project directory
 * @param {string} context.worktree - Git worktree
 * @returns {Object} Plugin hooks
 */
module.exports = function dotnetAgentHarnessPlugin(context) {
  const { project, client, $, directory, worktree } = context;
  
  console.log(`[${PLUGIN_NAME}] Plugin loaded`);
  
  return {
    // Install bundled content when plugin is initialized
    'plugin.install': async () => {
      console.log(`[${PLUGIN_NAME}] Installing bundled content...`);
      await installBundledContent(directory);
      console.log(`[${PLUGIN_NAME}] Installation complete`);
    },
    
    // Re-install on project updates
    'installation.updated': async () => {
      console.log(`[${PLUGIN_NAME}] Re-installing bundled content...`);
      await installBundledContent(directory);
    },
    
    // Provide info about the plugin
    'plugin.info': () => ({
      name: PLUGIN_NAME,
      version: '0.0.1',
      description: '.NET Agent Harness - 14 specialist agents, 131 skills, commands, and rules',
      agents: 14,
      skills: 131
    })
  };
};
