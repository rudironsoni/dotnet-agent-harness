import js from '@eslint/js';
import globals from 'globals';

export default [
  js.configs.recommended,
  {
    files: ['**/*.js', '**/*.mjs'],
    languageOptions: {
      ecmaVersion: 'latest',
      sourceType: 'module',
      globals: {
        ...globals.node,
        ...globals.es2021,
      },
    },
    rules: {
      'no-unused-vars': ['error', { argsIgnorePattern: '^_' }],
      'no-console': ['warn', { allow: ['log', 'warn', 'error'] }],
      'prefer-const': 'error',
      'no-var': 'error',
      'object-shorthand': 'error',
      'prefer-template': 'error',
    },
  },
  {
    files: ['scripts/**/*.js'],
    rules: {
      'no-console': 'off',
    },
  },
  {
    ignores: [
      'node_modules/**',
      'dist/**',
      '.opencode/**',
      '.claude/**',
      '.codex/**',
      '.gemini/**',
      '.agents/**',
      '.agent/**',
      'packages/dotnet-agent-harness-opencode/bundled/**',
      '**/*.d.ts',
      '**/*.ts',
    ],
  },
];
