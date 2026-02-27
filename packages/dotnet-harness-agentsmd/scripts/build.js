const path = require('path');
const { buildPlatformPackage } = require('../../../scripts/build/build_platform_package.cjs');

buildPlatformPackage({
  target: 'agentsmd',
  generatedDir: '.agents',
  rootFiles: ['AGENTS.md'],
  packageDir: path.join(__dirname, '..'),
}).catch(err => {
  console.error('Build failed:', err);
  process.exit(1);
});
