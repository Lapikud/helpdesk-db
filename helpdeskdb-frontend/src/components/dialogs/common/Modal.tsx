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
				className={`bg-white rounded-lg max-w-md w-full max-h-[90vh] flex flex-col ${className}`}
			>
				<div className="flex justify-end px-4 pt-3 shrink-0">
					<button
						onClick={onClose}
						className="text-orange-400 hover:text-gray-700 text-5xl leading-none"
						aria-label="Close"
					>
						&times;
					</button>
				</div>
				<div className="overflow-y-auto px-6 pb-6">
					{children}
				</div>
			</div>
		</div>
	);
};
