import { NextRequest, NextResponse } from "next/server";

// The Next server proxies /api/* to the backend (see rewrites() in next.config.ts), but its
// proxy does not add X-Forwarded-Proto on its own. Forward the browser's real scheme so the
// backend's UseForwardedHeaders sees HTTPS requests as HTTPS and marks the auth cookies Secure.
//
// When a TLS-terminating proxy (Caddy, nginx, ...) sits in front of this server, it already
// sets X-Forwarded-Proto to the browser's scheme while the hop to Next is plain HTTP — keep
// the proxy's value instead of clobbering it with "http". Spoofing is not a concern: in
// deployment the Next port is loopback/compose-network only, so only the proxy reaches it.
// Without a proxy (dev, dev:https) the header is absent and the request's own scheme is used.
export function middleware(request: NextRequest) {
	const headers = new Headers(request.headers);
	if (!headers.has("x-forwarded-proto")) {
		headers.set("x-forwarded-proto", request.nextUrl.protocol.replace(":", ""));
	}
	return NextResponse.next({ request: { headers } });
}

export const config = { matcher: "/api/:path*" };
