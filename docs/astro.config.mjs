// @ts-check
import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';

// https://astro.build/config
export default defineConfig({
	site: 'https://static-components.techgems.net',
	integrations: [
		starlight({
			title: 'Static Components',
			description: "Build component-driven UIs in ASP.NET Core without leaving Razor Pages — no JavaScript framework required.",
			logo: {
				src: "./src/assets/logo.svg",
				replacesTitle: true
			},
			favicon: './src/assets/favicon.ico',
			social: [
				{ icon: 'github', label: 'GitHub', href: 'https://github.com/techgems/static-components' }
			],
			head: [
				{
					tag: 'script',
					attrs: {
						src: 'https://cdn.usefathom.com/script.js',
						'data-site': 'MY-FATHOM-ID',
						defer: true,
					},
				},
			],
			components: {
				ThemeProvider: './src/components/DefaultDark.astro',
				ThemeSelect: './src/components/DisableThemeColor.astro',
			},
			sidebar: [
				{
					label: 'Introduction',
					items: [
						{ label: 'Overview', slug: 'introduction/overview' },
						{ label: 'Installation', slug: 'introduction/installation' },
						{ label: 'Basic Usage', slug: 'introduction/basic-usage' },
						{ label: 'HATS Stack', slug: 'introduction/hats-stack' },
						{ label: 'Comparisons', slug: 'introduction/comparisons' },
						{ label: 'Examples', slug: 'introduction/examples' }
					],
				},
				{
					label: 'Features',
					items: [
						{ label: 'Child Content and Slots', slug: 'features/child-content-and-slots' },
						{ label: 'Leaf Nodes', slug: 'features/leaf-nodes' },
						{ label: 'Static Scripts', slug: 'features/static-scripts' },
						{ label: 'JavaScript Object Serialization', slug: 'features/javascript-object-serialization' }
					]
				},
				{
					label: 'Advanced Usage',
					items: [
						{ label: 'Integrating AlpineJS', slug: 'advanced-usage/alpine' },
						{ label: 'Override Default Rendering', slug: 'advanced-usage/override-rendering' },
						{ label: 'Writing your own UI libraries', slug: 'advanced-usage/writing-your-own-ui-libraries' }
					]
				},
				{
					label: 'Miscellaneous',
					items: [
						{ label: 'AI Usage', slug: 'miscellaneous/using-ai' }
					]
				}
			],
		}),
	],
});
