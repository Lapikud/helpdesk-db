import type { NextConfig } from "next";

const nextConfig: NextConfig = {
	// output: "export",
	trailingSlash: true,
	// basePath: '/~maperm/helpdeskDb'
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
