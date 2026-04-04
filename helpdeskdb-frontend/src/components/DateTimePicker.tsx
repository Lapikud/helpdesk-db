"use client";

import { CalendarIcon } from "lucide-react";
import { format } from "date-fns";
import { cn } from "../lib/utils";
import { Button } from "./ui/button";
import { Calendar } from "./ui/calendar";
import { Popover, PopoverContent, PopoverTrigger } from "./ui/popover";
import { useEffect, useState } from "react";
import {
	Select,
	SelectContent,
	SelectItem,
	SelectTrigger,
	SelectValue,
} from "./ui/select";
import { ScrollArea } from "./ui/scroll-area";

interface DateTimePickerProps {
	value: Date | null;
	onChange: (date: Date | null) => void;
}

export function DateTimePicker({ value, onChange }: DateTimePickerProps) {
	const [isOpen, setIsOpen] = useState(false);
	const [date, setDate] = useState<Date | null>(value ?? null);
	const [time, setTime] = useState<string>(
		value ? format(value, "HH:mm") : "05:00",
	);

	// Keep local state in sync if parent controls the value
	useEffect(() => {
		if (value) {
			setDate(value);
			setTime(format(value, "HH:mm"));
		}
	}, [value]);

	const handleDateChange = (selectedDate: Date | undefined) => {
		if (!selectedDate) return;

		// Apply current time
		const [hours, minutes] = time.split(":");
		selectedDate.setHours(parseInt(hours), parseInt(minutes));

		setDate(selectedDate);
		onChange(selectedDate);
		setIsOpen(false);
	};

	const handleTimeChange = (selectedTime: string) => {
		setTime(selectedTime);
		if (!date) return;

		const [hours, minutes] = selectedTime.split(":");
		const newDate = new Date(date.getTime());
		newDate.setHours(parseInt(hours), parseInt(minutes));
		setDate(newDate);
		onChange(newDate);
	};

	return (
		<div className="flex w-full gap-4">
			<Popover open={isOpen} onOpenChange={setIsOpen}>
				<PopoverTrigger asChild>
					<Button
						variant={"outline"}
						className={cn(
							"w-full font-normal",
							!date && "text-muted-foreground",
						)}
					>
						{date ? (
							`${format(date, "PPP")}, ${time}`
						) : (
							<span>Pick a date</span>
						)}
						<CalendarIcon className="ml-auto h-4 w-4 opacity-50" />
					</Button>
				</PopoverTrigger>
				<PopoverContent className="w-auto p-0" align="start">
					<Calendar
						mode="single"
						captionLayout="dropdown"
						selected={date || undefined}
						onSelect={handleDateChange}
						onDayClick={() => setIsOpen(false)}
						startMonth={new Date(2000, 0)}
						endMonth={new Date(new Date().getFullYear(), 11)}
						disabled={(date) =>
							Number(date) < Date.now() - 1000 * 60 * 60 * 24 ||
							Number(date) > Date.now() + 1000 * 60 * 60 * 24 * 30
						}
						defaultMonth={date ?? undefined}
					/>
				</PopoverContent>
			</Popover>

			<Select value={time} onValueChange={handleTimeChange}>
				<SelectTrigger className="font-normal focus:ring-0 w-[120px] focus:ring-offset-0">
					<SelectValue />
				</SelectTrigger>
				<SelectContent>
					<ScrollArea className="h-[15rem]">
						{Array.from({ length: 144 }).map((_, i) => {
							const hour = Math.floor(i / 6)
								.toString()
								.padStart(2, "0");
							const minute = ((i % 6) * 10)
								.toString()
								.padStart(2, "0");
							return (
								<SelectItem key={i} value={`${hour}:${minute}`}>
									{hour}:{minute}
								</SelectItem>
							);
						})}
					</ScrollArea>
				</SelectContent>
			</Select>
		</div>
	);
}
