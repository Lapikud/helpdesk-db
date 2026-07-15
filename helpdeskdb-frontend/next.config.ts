import type { NextConfig } from "next";

const isDev = process.env.NODE_ENV !== "production";

// Content-Security-Policy. Next.js injects inline bootstrap scripts/styles, so 'unsafe-inline'
// is required for the app to run; in dev, Turbopack HMR additionally needs 'unsafe-eval' and a
// websocket connection back to the dev server.
const csp = [
	"default-src 'self'",
	`script-src 'self' 'unsafe-inline'${isDev ? " 'unsafe-eval'" : ""}`,
	"style-src 'self' 'unsafe-inline'",
	"img-src 'self' data: blob:",
	"font-src 'self'",
	`connect-src 'self'${isDev ? " ws://localhost:* wss://localhost:*" : ""}`,
	"frame-ancestors 'none'",
	"object-src 'none'",
	"base-uri 'self'",
	"form-action 'self'",
].join("; ");

const securityHeaders = [
	{ key: "Content-Security-Policy", value: csp },
	{ key: "X-Frame-Options", value: "DENY" },
	{ key: "X-Content-Type-Options", value: "nosniff" },
	{ key: "Referrer-Policy", value: "strict-origin-when-cross-origin" },
	{ key: "Permissions-Policy", value: "camera=(), microphone=(), geolocation=()" },
	// Only honored over HTTPS; harmless over the plain-HTTP dev server.
	{ key: "Strict-Transport-Security", value: "max-age=63072000; includeSubDomains" },
];

const nextConfig: NextConfig = {
	// output: "export",
	// Strip console.* from production bundles so entity data (users, reservations,
	// refresh-token records) never lands in end users' browser consoles.
	compiler: {
		removeConsole: isDev ? false : { exclude: ["error", "warn"] },
	},
	trailingSlash: true,
	skipTrailingSlashRedirect: true,
	// basePath: '/~maperm/helpdeskDb'
	async headers() {
		return [
			{
				source: "/:path*",
				headers: securityHeaders,
			},
		];
	},
	async rewrites() {
		return [
			{
				source: "/api/:path*",
				destination: `${process.env.NEXT_PUBLIC_BACKEND_URL}/api/:path*`,
			},
		];
	},
};

export default nextConfig;
