import { ReactNode } from "react";

interface ModalProps {
	open: boolean;
	onClose: () => void;
	children: ReactNode;
	className?: string;
}

export const Modal = ({
	open,
	onClose,
	children,
	className = "",
}: ModalProps) => {
	if (!open) return null;

	return (
		<div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
			<div
				className={`bg-white rounded-lg p-6 max-w-md w-full ${className}`}
			>
				<button
					onClick={onClose}
					className="absolute top-4 right-4 text-orange-400 hover:text-gray-700 text-5xl"
					aria-label="Close"
				>
					&times;
				</button>
				{children}
			</div>
		</div>
	);
};
