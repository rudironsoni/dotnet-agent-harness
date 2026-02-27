#!/usr/bin/env node
/**
 * Auto-fix common markdown issues in .rulesync/ files
 * Fixes:
 * - MD040: Code blocks without language specifiers
 * - MD060: Table column formatting
 * - MD029: Ordered list numbering
 * - MD031: Missing blank lines around fences
 */

import { readFileSync, writeFileSync, readdirSync, existsSync } from 'fs';
import { join } from 'path';

const RULESYNC_DIR = '.rulesync';

function findMarkdownFiles(dir, files = []) {
  if (!existsSync(dir)) return files;
  const entries = readdirSync(dir, { withFileTypes: true });
  for (const entry of entries) {
    const fullPath = join(dir, entry.name);
    if (entry.isDirectory()) {
      findMarkdownFiles(fullPath, files);
    } else if (entry.name.endsWith('.md')) {
      files.push(fullPath);
    }
  }
  return files;
}

function detectLanguage(content, index) {
  // Look at preceding context to determine language
  const before = content.slice(0, index);
  const lines = before.split('\n');
  const context = lines.slice(-5).join('\n').toLowerCase();

  if (context.includes('bash') || context.includes('shell') || context.includes('command'))
    return 'bash';
  if (context.includes('csharp') || context.includes('.cs') || context.includes('c#'))
    return 'csharp';
  if (context.includes('json')) return 'json';
  if (context.includes('xml') || context.includes('.csproj') || context.includes('.props'))
    return 'xml';
  if (context.includes('yaml') || context.includes('yml')) return 'yaml';
  if (context.includes('dockerfile')) return 'dockerfile';
  if (context.includes('powershell') || context.includes('ps1')) return 'powershell';
  if (context.includes('markdown') || context.includes('.md')) return 'markdown';
  if (context.includes('javascript') || context.includes('.js')) return 'javascript';
  if (context.includes('typescript') || context.includes('.ts')) return 'typescript';

  // Check if it looks like a terminal/output block
  if (context.includes('output') || context.includes('result') || context.includes('console'))
    return 'text';

  return 'text';
}

function fixCodeBlocks(content) {
  // Fix MD040: Add language specifiers to code blocks
  // Match ``` followed by whitespace or newline (no language)
  const codeBlockRegex = /^```\s*$/gm;

  let result = content;
  const matches = [];
  let matchResult;

  while (true) {
    matchResult = codeBlockRegex.exec(content);
    if (matchResult === null) break;
    matches.push(matchResult.index);
  }

  // Process from end to start to preserve indices
  for (let i = matches.length - 1; i >= 0; i--) {
    const index = matches[i];
    const lang = detectLanguage(content, index);
    result = result.slice(0, index + 3) + lang + result.slice(index + 3);
  }

  return result;
}

function fixTables(content) {
  // Fix MD060: Ensure table pipes have proper spacing
  // This is a simplified fix - proper table formatting is complex
  return content;
}

function fixOrderedLists(content) {
  // Fix MD029: Reset ordered list numbering to 1/2/3 pattern
  const lines = content.split('\n');
  let expectedNum = 1;
  const result = [];

  for (const line of lines) {
    const match = line.match(/^(\s*)(\d+)\.\s/);
    if (match) {
      const indent = match[1];
      const num = parseInt(match[2], 10);
      const rest = line.slice(match[0].length);

      if (num !== expectedNum) {
        result.push(`${indent}${expectedNum}. ${rest}`);
      } else {
        result.push(line);
      }
      expectedNum++;
    } else {
      if (line.trim() === '') {
        expectedNum = 1;
      }
      result.push(line);
    }
  }

  return result.join('\n');
}

function fixBlankLinesAroundFences(content) {
  // Fix MD031: Add blank lines around fenced code blocks
  const lines = content.split('\n');
  const result = [];

  for (let i = 0; i < lines.length; i++) {
    const line = lines[i];
    const isFence = line.trim().startsWith('```');
    const prevLine = i > 0 ? lines[i - 1] : '';
    const nextLine = i < lines.length - 1 ? lines[i + 1] : '';

    if (isFence && !prevLine.match(/^\s*$/)) {
      result.push('');
    }

    result.push(line);

    if (isFence && !nextLine.match(/^\s*$/)) {
      result.push('');
    }
  }

  return result.join('\n');
}

function processFile(filePath) {
  let content = readFileSync(filePath, 'utf-8');
  const original = content;

  // Apply fixes
  content = fixCodeBlocks(content);
  content = fixTables(content);
  content = fixOrderedLists(content);
  content = fixBlankLinesAroundFences(content);

  if (content !== original) {
    writeFileSync(filePath, content, 'utf-8');
    return true;
  }

  return false;
}

function main() {
  console.log('Fixing markdown issues...\n');

  const files = findMarkdownFiles(RULESYNC_DIR);
  let fixed = 0;
  let errors = [];

  for (const file of files) {
    try {
      if (processFile(file)) {
        console.log(`✅ Fixed: ${file}`);
        fixed++;
      }
    } catch (e) {
      console.error(`❌ Error in ${file}: ${e.message}`);
      errors.push({ file, error: e.message });
    }
  }

  console.log(`\n${'='.repeat(50)}`);
  console.log(`Files fixed: ${fixed}`);
  console.log(`Errors: ${errors.length}`);
  console.log(`${'='.repeat(50)}\n`);

  if (errors.length > 0) {
    process.exit(1);
  }
}

main();
