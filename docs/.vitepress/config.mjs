import { defineConfig } from 'vitepress';

export default defineConfig({
  title: 'dotnet-agent-harness',
  description:
    'Comprehensive .NET development guidance for modern C#, ASP.NET Core, MAUI, Blazor, and cloud-native apps',
  base: '/dotnet-agent-harness/',

  themeConfig: {
    nav: [
      { text: 'Home', link: '/' },
      { text: 'Consumer Guide', link: '/guide/' },
      { text: 'Maintainer Guide', link: '/maintainer/' },
      { text: 'Skills', link: '/skills/' },
    ],

    sidebar: {
      '/guide/': [
        {
          text: 'Consumer Guide',
          items: [
            { text: 'Getting Started', link: '/guide/' },
            { text: 'Installation', link: '/guide/installation' },
            { text: 'Commands', link: '/guide/commands' },
            { text: 'Troubleshooting', link: '/guide/troubleshooting' },
          ],
        },
      ],

      '/maintainer/': [
        {
          text: 'Maintainer Guide',
          items: [
            { text: 'Overview', link: '/maintainer/' },
            { text: 'RuleSync Authoring', link: '/maintainer/rulesync-authoring' },
            { text: 'Bundle Generation', link: '/maintainer/bundle-generation' },
            { text: 'Release Workflow', link: '/maintainer/release-workflow' },
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
