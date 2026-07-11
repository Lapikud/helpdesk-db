"use client";

import { useEffect, useRef } from "react";

// Max ms between keystrokes to still count as scanner input (scanners emit ~10-30ms/char)
const MAX_KEY_GAP_MS = 50;
// Minimum decoded length to treat a burst as a barcode
const MIN_BARCODE_LENGTH = 4;

interface UseBarcodeScannerOptions {
	onScan: (code: string) => void;
	enabled?: boolean;
}

function isEditableElement(el: Element | null): boolean {
	if (!el) return false;
	const tag = el.tagName;
	return (
		tag === "INPUT" ||
		tag === "TEXTAREA" ||
		tag === "SELECT" ||
		(el as HTMLElement).isContentEditable
	);
}

/**
 * Detects input from a keyboard-wedge (USB/handheld) barcode scanner:
 * a rapid burst of printable characters terminated by Enter.
 *
 * Does nothing while focus is in an editable element, so scanners keep
 * their native behavior when a form field (e.g. a dialog's barcode
 * field) is focused. An editable element can opt back in by carrying a
 * `data-barcode-scan-target` attribute — a scan detected while it is
 * focused fires onScan with only the burst characters, letting the
 * caller replace the field's content instead of appending to it.
 */
export function useBarcodeScanner({
	onScan,
	enabled = true,
}: UseBarcodeScannerOptions) {
	const bufferRef = useRef("");
	const lastKeyTimeRef = useRef(0);
	const onScanRef = useRef(onScan);
	onScanRef.current = onScan;

	useEffect(() => {
		if (!enabled) return;

		const handleKeyDown = (e: KeyboardEvent) => {
			const active = document.activeElement;
			if (
				isEditableElement(active) &&
				!active!.hasAttribute("data-barcode-scan-target")
			) {
				bufferRef.current = "";
				return;
			}

			const now = Date.now();
			if (now - lastKeyTimeRef.current > MAX_KEY_GAP_MS) {
				bufferRef.current = "";
			}
			lastKeyTimeRef.current = now;

			if (e.key === "Enter") {
				if (bufferRef.current.length >= MIN_BARCODE_LENGTH) {
					e.preventDefault();
					onScanRef.current(bufferRef.current);
				}
				bufferRef.current = "";
				return;
			}

			if (e.key.length === 1 && !e.ctrlKey && !e.altKey && !e.metaKey) {
				bufferRef.current += e.key;
			}
		};

		window.addEventListener("keydown", handleKeyDown);
		return () => window.removeEventListener("keydown", handleKeyDown);
	}, [enabled]);
}
