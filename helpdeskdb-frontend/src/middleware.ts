import { NextRequest, NextResponse } from "next/server";

// The Next server acts as the TLS-terminating reverse proxy for /api/* (see rewrites() in
// next.config.ts), but its proxy does not add X-Forwarded-Proto on its own. Forward the
// browser's real scheme so the backend's UseForwardedHeaders sees HTTPS requests as HTTPS
// and marks the auth cookies Secure. Over plain http this forwards "http" — no-op.
export function middleware(request: NextRequest) {
	const headers = new Headers(request.headers);
	headers.set("x-forwarded-proto", request.nextUrl.protocol.replace(":", ""));
	return NextResponse.next({ request: { headers } });
}

export const config = { matcher: "/api/:path*" };
