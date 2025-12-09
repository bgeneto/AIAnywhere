import ReactMarkdown from 'react-markdown';
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter';
import { oneDark } from 'react-syntax-highlighter/dist/esm/styles/prism';
import remarkMath from 'remark-math';
import remarkGfm from 'remark-gfm';
import remarkBreaks from 'remark-breaks';
import rehypeKatex from 'rehype-katex';
import 'katex/dist/katex.min.css';

interface MarkdownRendererProps {
    content: string;
    className?: string;
    id?: string;
}

export function MarkdownRenderer({ content, className = '', id }: MarkdownRendererProps) {
    return (
        <div
            className={`text-slate-900 dark:text-white font-serif
                  [&_.katex-display]:my-3 [&_.katex-display]:text-base
                  [&_.katex]:text-[0.85em] [&_.katex]:text-inherit
                  ${className}`}
            id={id}
        >
            <ReactMarkdown
                remarkPlugins={[remarkMath, remarkGfm, remarkBreaks]}
                rehypePlugins={[rehypeKatex]}
                components={{
                    h1: ({ children }) => (
                        <h1 className="text-3xl font-bold mt-6 mb-3 text-slate-900 dark:text-white font-sans">{children}</h1>
                    ),
                    h2: ({ children }) => (
                        <h2 className="text-2xl font-bold mt-5 mb-2 text-slate-900 dark:text-white font-sans">{children}</h2>
                    ),
                    h3: ({ children }) => (
                        <h3 className="text-xl font-semibold mt-4 mb-2 text-slate-900 dark:text-white font-sans">{children}</h3>
                    ),
                    h4: ({ children }) => (
                        <h4 className="text-lg font-semibold mt-3 mb-1 text-slate-900 dark:text-white font-sans">{children}</h4>
                    ),
                    p: ({ children }) => (
                        <p className="text-base leading-relaxed my-2 text-slate-700 dark:text-white">{children}</p>
                    ),
                    ul: ({ children }) => (
                        <ul className="text-base leading-relaxed my-2 ml-4 list-disc text-slate-700 dark:text-white">{children}</ul>
                    ),
                    ol: ({ children }) => (
                        <ol className="text-base leading-relaxed my-2 ml-4 list-decimal text-slate-700 dark:text-white">{children}</ol>
                    ),
                    li: ({ children }) => (
                        <li className="my-1">{children}</li>
                    ),
                    blockquote: ({ children }) => (
                        <blockquote className="border-l-4 border-slate-300 dark:border-slate-600 pl-4 my-3 italic text-slate-600 dark:text-slate-400">{children}</blockquote>
                    ),
                    a: ({ href, children }) => (
                        <a href={href} className="text-blue-600 dark:text-blue-400 hover:underline" target="_blank" rel="noopener noreferrer">{children}</a>
                    ),
                    strong: ({ children }) => (
                        <strong className="font-bold text-slate-900 dark:text-white">{children}</strong>
                    ),
                    em: ({ children }) => (
                        <em className="italic">{children}</em>
                    ),
                    table: ({ children }) => (
                        <div className="overflow-x-auto my-4">
                            <table className="min-w-full border-collapse border border-slate-300 dark:border-slate-600">
                                {children}
                            </table>
                        </div>
                    ),
                    thead: ({ children }) => (
                        <thead className="bg-slate-100 dark:bg-slate-700">
                            {children}
                        </thead>
                    ),
                    tbody: ({ children }) => (
                        <tbody className="divide-y divide-slate-200 dark:divide-slate-600">
                            {children}
                        </tbody>
                    ),
                    tr: ({ children }) => (
                        <tr className="even:bg-slate-50 dark:even:bg-slate-800/50">
                            {children}
                        </tr>
                    ),
                    th: ({ children }) => (
                        <th className="px-3 py-2 text-left text-base font-semibold text-slate-900 dark:text-white border border-slate-300 dark:border-slate-600">
                            {children}
                        </th>
                    ),
                    td: ({ children }) => (
                        <td className="px-3 py-2 text-base text-slate-700 dark:text-white border border-slate-300 dark:border-slate-600">
                            {children}
                        </td>
                    ),
                    code({ className, children, ...props }) {
                        const match = /language-(\w+)/.exec(className || '');
                        const isInline = !match && !className;
                        return !isInline ? (
                            <SyntaxHighlighter
                                style={oneDark as { [key: string]: React.CSSProperties }}
                                language={match ? match[1] : 'text'}
                                PreTag="div"
                                className="rounded-lg !mt-2 !mb-2 font-mono"
                            >
                                {String(children).replace(/\n$/, '')}
                            </SyntaxHighlighter>
                        ) : (
                            <code className="bg-slate-200 dark:bg-slate-700 px-1 py-0.5 rounded text-sm font-mono" {...props}>
                                {children}
                            </code>
                        );
                    },
                }}
            >
                {content}
            </ReactMarkdown>
        </div>
    );
}
