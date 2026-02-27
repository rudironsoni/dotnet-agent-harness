const path = require('path');
const { buildPlatformPackage } = require('../../../scripts/build/build_platform_package.cjs');

buildPlatformPackage({
  target: 'antigravity',
  generatedDir: '.agent',
  packageDir: path.join(__dirname, '..'),
}).catch(err => {
  console.error('Build failed:', err);
  process.exit(1);
});
