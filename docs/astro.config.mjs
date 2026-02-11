// @ts-check
import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';

// https://astro.build/config
export default defineConfig({
	site: 'https://static-components.techgems.net',
	integrations: [
		starlight({
			title: 'Static Components',
			description: "Static Components is an ASP.NET Core library that allows you to create modern UI reusable elements with Razor Pages.",
			logo: {
				src: "./src/assets/logo.svg",
				replacesTitle: true
			},
			favicon: 'favicon.svg',
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
						{ label: 'Comparisons', slug: 'introduction/comparisons' }
					],
				},
				{
					label: 'Advanced Usage',
					items: [
						{ label: 'Child Content and Slots', slug: 'advanced-usage/child-content-and-slots' },
						{ label: 'Override Default Rendering', slug: 'advanced-usage/override-rendering' },
						{ label: 'AlpineJS', slug: 'advanced-usage/alpine' },
						//{ label: 'HTMX', slug: 'advanced-usage/htmx' }
					]
				},
				/*
				{
					label: 'How to build a UI library',
					autogenerate: { directory: 'guide' },
				},*/
			],
		}),
	],
});
