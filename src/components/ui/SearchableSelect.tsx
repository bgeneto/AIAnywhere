import React, { useState, useRef, useEffect, useMemo } from 'react';

export interface Option {
    value: string;
    label: string;
    icon?: React.ReactNode;
    group?: string;
}

interface SearchableSelectProps {
    value: string;
    onChange: (value: string) => void;
    options: Option[];
    placeholder?: string;
    className?: string;
}

export function SearchableSelect({ value, onChange, options, placeholder = 'Select...', className = '' }: SearchableSelectProps) {
    const [isOpen, setIsOpen] = useState(false);
    const [searchQuery, setSearchQuery] = useState('');
    const [highlightedIndex, setHighlightedIndex] = useState(-1);
    const containerRef = useRef<HTMLDivElement>(null);
    const searchInputRef = useRef<HTMLInputElement>(null);
    const listRef = useRef<HTMLDivElement>(null);
    const itemRefs = useRef<(HTMLButtonElement | null)[]>([]);

    // Close when clicking outside
    useEffect(() => {
        function handleClickOutside(event: MouseEvent) {
            if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
                setIsOpen(false);
            }
        }
        document.addEventListener('mousedown', handleClickOutside);
        return () => document.removeEventListener('mousedown', handleClickOutside);
    }, []);

    // Focus search input when opening and scroll to selected
    useEffect(() => {
        if (isOpen) {
            if (searchInputRef.current) {
                searchInputRef.current.focus();
            }
            // clear search query when opening in order to show all options
            setSearchQuery('');

            // Highlight selected option
            const selectedIndex = options.findIndex(opt => opt.value === value);
            setHighlightedIndex(selectedIndex >= 0 ? selectedIndex : 0);
        }
    }, [isOpen, value, options]);

    const selectedOption = options.find(opt => opt.value === value);

    const filteredOptions = useMemo(() => {
        if (!searchQuery) return options;
        const lowerQuery = searchQuery.toLowerCase();
        return options.filter(opt =>
            opt.label.toLowerCase().includes(lowerQuery)
        );
    }, [options, searchQuery]);

    // Update highlighted index when filter changes
    useEffect(() => {
        setHighlightedIndex(0);
    }, [searchQuery]);

    // Scroll highlighted item into view
    useEffect(() => {
        if (isOpen && highlightedIndex >= 0 && itemRefs.current[highlightedIndex]) {
            itemRefs.current[highlightedIndex]?.scrollIntoView({
                block: 'nearest',
            });
        }
    }, [highlightedIndex, isOpen]);

    // Group options
    const groupedOptions = useMemo(() => {
        const groups: Record<string, Option[]> = {};
        const noGroup: Option[] = [];

        filteredOptions.forEach(opt => {
            if (opt.group) {
                if (!groups[opt.group]) groups[opt.group] = [];
                groups[opt.group].push(opt);
            } else {
                noGroup.push(opt);
            }
        });

        return { noGroup, groups };
    }, [filteredOptions]);

    // Flattened list for keyboard navigation mapping
    const flattenedOptions = useMemo(() => {
        const flat: Option[] = [];
        flat.push(...groupedOptions.noGroup);
        Object.values(groupedOptions.groups).forEach(groupOpts => {
            flat.push(...groupOpts);
        });
        return flat;
    }, [groupedOptions]);

    const handleKeyDown = (e: React.KeyboardEvent) => {
        if (!isOpen) {
            if (e.key === 'Enter' || e.key === ' ' || e.key === 'ArrowDown') {
                e.preventDefault();
                setIsOpen(true);
            }
            return;
        }

        switch (e.key) {
            case 'ArrowDown':
                e.preventDefault();
                setHighlightedIndex(prev =>
                    prev < flattenedOptions.length - 1 ? prev + 1 : 0
                );
                break;
            case 'ArrowUp':
                e.preventDefault();
                setHighlightedIndex(prev =>
                    prev > 0 ? prev - 1 : flattenedOptions.length - 1
                );
                break;
            case 'Enter':
                e.preventDefault();
                if (flattenedOptions[highlightedIndex]) {
                    onChange(flattenedOptions[highlightedIndex].value);
                    setIsOpen(false);
                }
                break;
            case 'Escape':
                e.preventDefault();
                setIsOpen(false);
                break;
        }
    };

    return (
        <div
            className={`relative ${className}`}
            ref={containerRef}
            onKeyDown={handleKeyDown}
        >
            {/* Trigger */}
            <button
                type="button"
                onClick={() => setIsOpen(!isOpen)}
                className="w-full px-4 py-3 text-left text-sm rounded-lg border border-slate-300 dark:border-slate-600 
                   bg-white dark:bg-slate-800 text-slate-900 dark:text-white
                   focus:ring-2 focus:ring-blue-500 focus:border-transparent
                   transition-colors duration-200 flex items-center justify-between"
            >
                <span className="truncate flex-1">
                    {selectedOption ? (
                        <span className="flex items-center gap-2">
                            {selectedOption.icon && <span className="flex-shrink-0 text-slate-500 dark:text-slate-400">{selectedOption.icon}</span>}
                            <span>{selectedOption.label}</span>
                        </span>
                    ) : (
                        <span className="text-slate-500 dark:text-slate-400">{placeholder}</span>
                    )}
                </span>
                <span className="ml-2 text-slate-400 flex-shrink-0">
                    <svg className={`w-4 h-4 transition-transform duration-200 ${isOpen ? 'rotate-180' : ''}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                    </svg>
                </span>
            </button>

            {/* Dropdown */}
            {isOpen && (
                <div className="absolute z-50 w-full mt-1 bg-white dark:bg-slate-800 rounded-lg shadow-xl border border-slate-200 dark:border-slate-700 max-h-80 flex flex-col overflow-hidden">
                    {/* Search Input */}
                    <div className="p-2 border-b border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 z-10">
                        <div className="relative">
                            <span className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400">
                                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                                </svg>
                            </span>
                            <input
                                ref={searchInputRef}
                                type="text"
                                value={searchQuery}
                                onChange={(e) => setSearchQuery(e.target.value)}
                                placeholder="Search..."
                                className="w-full pl-9 pr-3 py-2 text-sm rounded-md border border-slate-300 dark:border-slate-600 
                           bg-slate-50 dark:bg-slate-900 text-slate-900 dark:text-white
                           focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none
                           placeholder-slate-400 dark:placeholder-slate-500"
                                onClick={(e) => e.stopPropagation()}
                            />
                        </div>
                    </div>

                    {/* List */}
                    <div ref={listRef} className="overflow-y-auto flex-1 p-1 custom-scrollbar">
                        {groupedOptions.noGroup.map((opt, index) => {
                            const globalIndex = index;
                            return (
                                <OptionItem
                                    key={opt.value}
                                    option={opt}
                                    isSelected={value === opt.value}
                                    isHighlighted={highlightedIndex === globalIndex}
                                    ref={(el) => { itemRefs.current[globalIndex] = el; }}
                                    onClick={() => {
                                        onChange(opt.value);
                                        setIsOpen(false);
                                    }}
                                />
                            );
                        })}

                        {Object.entries(groupedOptions.groups).map(([groupName, groupOptions], groupIndex) => {
                            // Calculate offset for this group
                            const groupOffset = groupedOptions.noGroup.length +
                                Object.values(groupedOptions.groups)
                                    .slice(0, groupIndex)
                                    .reduce((acc, curr) => acc + curr.length, 0);

                            return (
                                <div key={groupName}>
                                    <div className="px-2 py-1.5 text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider bg-slate-50 dark:bg-slate-800/50 mt-1 first:mt-0 rounded-sm">
                                        {groupName}
                                    </div>
                                    {groupOptions.map((opt, optIndex) => {
                                        const globalIndex = groupOffset + optIndex;
                                        return (
                                            <OptionItem
                                                key={opt.value}
                                                option={opt}
                                                isSelected={value === opt.value}
                                                isHighlighted={highlightedIndex === globalIndex}
                                                ref={(el) => { itemRefs.current[globalIndex] = el; }}
                                                onClick={() => {
                                                    onChange(opt.value);
                                                    setIsOpen(false);
                                                }}
                                            />
                                        );
                                    })}
                                </div>
                            );
                        })}

                        {filteredOptions.length === 0 && (
                            <div className="px-4 py-8 text-sm text-slate-500 dark:text-slate-400 text-center flex flex-col items-center gap-2">
                                <svg className="w-8 h-8 opacity-50" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9.172 16.172a4 4 0 015.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                                </svg>
                                <span>No results found</span>
                            </div>
                        )}
                    </div>
                </div>
            )}
        </div>
    );
}

const OptionItem = React.forwardRef<HTMLButtonElement, { option: Option, isSelected: boolean, isHighlighted: boolean, onClick: () => void }>(
    ({ option, isSelected, isHighlighted, onClick }, ref) => {
        return (
            <button
                ref={ref}
                type="button"
                onClick={onClick}
                className={`w-full text-left px-3 py-2.5 text-sm rounded-md transition-colors flex items-center gap-3
          ${isSelected
                        ? 'bg-blue-50 dark:bg-blue-900/30 text-blue-700 dark:text-blue-300'
                        : isHighlighted
                            ? 'bg-slate-100 dark:bg-slate-700 text-slate-900 dark:text-white'
                            : 'text-slate-700 dark:text-slate-200 hover:bg-slate-100 dark:hover:bg-slate-700/50'
                    }`}
            >
                {option.icon && (
                    <span className={`flex-shrink-0 ${isSelected ? 'text-blue-600 dark:text-blue-400' : 'text-slate-400 dark:text-slate-500'}`}>
                        {option.icon}
                    </span>
                )}
                <span className="truncate flex-1">{option.label}</span>
                {isSelected && (
                    <span className="flex-shrink-0 text-blue-600 dark:text-blue-400">
                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                        </svg>
                    </span>
                )}
            </button>
        );
    }
);
OptionItem.displayName = 'OptionItem';
