import { useI18n } from '../../i18n/index';
import { openUrl } from '@tauri-apps/plugin-opener';
import appIcon from '../../assets/icon.png';

export function AboutPage() {
    const { t } = useI18n();

    const handleOpenGithub = async () => {
        try {
            await openUrl('https://github.com/bgeneto/AIAnywhere');
        } catch (error) {
            console.error('Failed to open URL:', error);
        }
    };

    return (
        <div className="flex flex-col h-full">
            {/* Page Header */}
            <div className="p-6 border-b border-slate-200 dark:border-slate-800">
                <h2 className="page-title">
                    {t.about.title}
                </h2>
            </div>

            {/* Content */}
            <div className="flex-1 overflow-y-auto p-6">
                <div className="max-w-2xl space-y-8">
                    {/* App Info */}
                    <div className="text-center space-y-4">
                        <img
                            src={appIcon}
                            alt="AI Anywhere"
                            className="w-24 h-24 mx-auto rounded-2xl shadow-lg"
                        />
                        <div>
                            <h1 className="text-3xl font-bold text-slate-900 dark:text-white">
                                {t.appName}
                            </h1>
                            <p className="text-slate-500 dark:text-slate-400">
                                {t.about.version} 1.3.2
                            </p>
                        </div>
                    </div>

                    {/* Description */}
                    <div className="space-y-3">
                        <p className="text-slate-700 dark:text-slate-300 text-center">
                            {t.about.description}
                        </p>
                    </div>

                    {/* Features */}
                    <div className="space-y-4">
                        <h3 className="section-title">
                            {t.about.features}
                        </h3>
                        <ul className="space-y-2">
                            {t.about.featureList.map((feature, index) => (
                                <li
                                    key={index}
                                    className="settings-row flex items-start gap-3"
                                >
                                    <span className="text-green-500">✓</span>
                                    <span className="text-sm text-slate-700 dark:text-slate-300">
                                        {feature}
                                    </span>
                                </li>
                            ))}
                        </ul>
                    </div>

                    {/* Credits */}
                    <div className="space-y-4 pt-4 border-t border-slate-200 dark:border-slate-800">
                        <h3 className="section-title">
                            {t.about.credits}
                        </h3>

                        <div className="space-y-3">
                            <div className="settings-row">
                                <div className="form-label">
                                    {t.about.developers}
                                </div>
                                <div className="text-sm text-slate-500 dark:text-slate-400 mt-1">
                                    Bernhard Enders (bgeneto)
                                </div>
                            </div>

                            <div className="settings-row">
                                <div className="form-label">
                                    {t.about.institution}
                                </div>
                                <div className="text-sm text-slate-500 dark:text-slate-400 mt-1">
                                    Universidade de Brasília (UnB)
                                </div>
                            </div>
                        </div>
                    </div>

                    {/* Repository */}
                    <div className="space-y-4 pt-4 border-t border-slate-200 dark:border-slate-800">
                        <h3 className="section-title">
                            {t.about.repository}
                        </h3>

                        <button
                            onClick={handleOpenGithub}
                            className="flex items-center gap-3 w-full p-4 
                         bg-slate-900 dark:bg-slate-800 hover:bg-slate-800 dark:hover:bg-slate-700
                         text-white rounded-lg transition-colors"
                        >
                            <svg className="w-6 h-6" fill="currentColor" viewBox="0 0 24 24">
                                <path fillRule="evenodd" clipRule="evenodd" d="M12 2C6.477 2 2 6.477 2 12c0 4.42 2.865 8.17 6.839 9.49.5.092.682-.217.682-.482 0-.237-.008-.866-.013-1.7-2.782.604-3.369-1.34-3.369-1.34-.454-1.156-1.11-1.462-1.11-1.462-.908-.62.069-.608.069-.608 1.003.07 1.531 1.03 1.531 1.03.892 1.529 2.341 1.087 2.91.831.092-.646.35-1.086.636-1.336-2.22-.253-4.555-1.11-4.555-4.943 0-1.091.39-1.984 1.029-2.683-.103-.253-.446-1.27.098-2.647 0 0 .84-.269 2.75 1.025A9.578 9.578 0 0112 6.836c.85.004 1.705.114 2.504.336 1.909-1.294 2.747-1.025 2.747-1.025.546 1.377.203 2.394.1 2.647.64.699 1.028 1.592 1.028 2.683 0 3.842-2.339 4.687-4.566 4.935.359.309.678.919.678 1.852 0 1.336-.012 2.415-.012 2.743 0 .267.18.578.688.48C19.138 20.167 22 16.418 22 12c0-5.523-4.477-10-10-10z" />
                            </svg>
                            <div className="text-left">
                                <div className="font-medium">{t.about.viewOnGithub}</div>
                                <div className="text-sm text-slate-400">https://github.com/bgeneto/AIAnywhere</div>
                            </div>
                            <span className="ml-auto">→</span>
                        </button>
                    </div>

                    {/* Tech Stack */}
                    <div className="space-y-4 pt-4 border-t border-slate-200 dark:border-slate-800">
                        <h3 className="section-title">
                            Built With
                        </h3>
                        <div className="flex flex-wrap gap-2">
                            {['Tauri 2.0', 'Rust', 'React', 'TypeScript', 'Tailwind CSS'].map((tech) => (
                                <span
                                    key={tech}
                                    className="badge-primary px-3 py-1.5 rounded-full"
                                >
                                    {tech}
                                </span>
                            ))}
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}
