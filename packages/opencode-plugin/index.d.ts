/**
 * dotnet-agent-harness OpenCode Plugin
 *
 * Type definitions for the OpenCode plugin
 */

export interface PluginContext {
  project: {
    name: string;
    path: string;
  };
  client: any;
  $: any;
  directory: string;
  worktree: string;
}

export interface PluginInfo {
  name: string;
  version: string;
  description: string;
  agents: number;
  skills: number;
}

export interface PluginHooks {
  'plugin.install'?: () => Promise<void>;
  'installation.updated'?: () => Promise<void>;
  'plugin.info'?: () => PluginInfo;
}

export type DotnetAgentHarnessPlugin = (context: PluginContext) => PluginHooks;

declare const plugin: DotnetAgentHarnessPlugin;
export = plugin;
