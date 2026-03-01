import { defineConfig } from 'vitepress';

export default defineConfig({
  title: 'dotnet-agent-harness',
  description:
    'Comprehensive .NET development guidance for modern C#, ASP.NET Core, MAUI, Blazor, and cloud-native apps',
  base: '/dotnet-agent-harness/',

  themeConfig: {
    nav: [
      { text: 'Home', link: '/' },
      { text: 'Guide', link: '/guide/' },
      { text: 'Skills', link: '/skills/' },
      { text: 'API', link: '/api/' },
    ],

    sidebar: {
      '/guide/': [
        {
          text: 'Getting Started',
          items: [
            { text: 'Installation', link: '/guide/installation' },
            { text: 'Quick Start', link: '/guide/quickstart' },
            { text: 'Configuration', link: '/guide/configuration' },
          ],
        },
        {
          text: 'Architecture',
          items: [
            { text: 'Overview', link: '/guide/architecture' },
            { text: 'Skills', link: '/guide/skills' },
            { text: 'Subagents', link: '/guide/subagents' },
            { text: 'Commands', link: '/guide/commands' },
          ],
        },
        {
          text: 'Advanced',
          items: [
            { text: 'MCP Integration', link: '/guide/mcp' },
            { text: 'Hooks', link: '/guide/hooks' },
            { text: 'Templates', link: '/guide/templates' },
          ],
        },
      ],

      '/skills/': [
        {
          text: 'Categories',
          items: [
            { text: 'All Skills', link: '/skills/' },
            { text: 'UI Frameworks', link: '/skills/ui' },
            { text: 'Data Access', link: '/skills/data' },
            { text: 'Testing', link: '/skills/testing' },
            { text: 'DevOps', link: '/skills/devops' },
          ],
        },
      ],
    },

    socialLinks: [{ icon: 'github', link: 'https://github.com/rudironsoni/dotnet-agent-harness' }],

    editLink: {
      pattern: 'https://github.com/rudironsoni/dotnet-agent-harness/edit/main/docs/:path',
    },

    search: {
      provider: 'local',
    },

    outline: {
      level: 'deep',
    },
  },

  markdown: {
    theme: {
      light: 'github-light',
      dark: 'github-dark',
    },
    lineNumbers: true,
    config: md => {
      // Mermaid support
      md.use(require('markdown-it-mermaid'));
    },
  },

  head: [
    ['link', { rel: 'icon', href: '/favicon.ico' }],
    ['meta', { name: 'theme-color', content: '#512BD4' }],
  ],

  sitemap: {
    hostname: 'https://rudironsoni.github.io',
  },
});
