// AI Anywhere - Internationalization Support
// Translations for English and Portuguese (Brazil)

export type Language = 'en' | 'pt-BR';

export interface Translations {
  // App
  appName: string;

  // Navigation
  nav: {
    home: string;
    history: string;
    customTasks: string;
    settings: string;
    apiSettings: string;
    appSettings: string;
    languageSettings: string;
    audioSettings: string;
    about: string;
  };

  // Home Page
  home: {
    title: string;
    taskSelection: string;
    promptContent: string;
    clear: string;
    send: string;
    cancel: string;
    processing: string;
    enterPrompt: string;
    customTasks: string;
    generating: string;
    waitingForResponse: string;
    promptHint: string;
  };

  // Operations
  operations: {
    customTask: string;
    customTaskDesc: string;
    emailReply: string;
    emailReplyDesc: string;
    imageGeneration: string;
    imageGenerationDesc: string;
    speechToText: string;
    speechToTextDesc: string;
    textToSpeech: string;
    textToSpeechDesc: string;
    textRewrite: string;
    textRewriteDesc: string;
    textTranslation: string;
    textTranslationDesc: string;
    textSummarization: string;
    textSummarizationDesc: string;
    whatsAppResponse: string;
    whatsAppResponseDesc: string;
    unicodeSymbols: string;
    unicodeSymbolsDesc: string;
  };

  // Settings
  settings: {
    title: string;
    save: string;
    cancel: string;
    resetDefaults: string;

    // API Settings
    api: {
      title: string;
      configuration: string;
      endpoint: string;
      endpointPlaceholder: string;
      apiKey: string;
      apiKeyPlaceholder: string;
      show: string;
      hide: string;
      apiKeyHelp: string;
      textModel: string;
      imageModel: string;
      audioModel: string;
      ttsModel: string;
      refreshModels: string;
      modelsAvailable: string;
      testConnection: string;
      testSuccess: string;
      testFailed: string;
      saveApiSettings: string;
    };

    // Language Settings
    language: {
      title: string;
      appLanguage: string;
      appLanguageDesc: string;
      translationTarget: string;
      translationTargetDesc: string;
    };

    // Models Settings
    models: {
      title: string;
    };

    // Audio Settings
    audio: {
      title: string;
      voice: string;
      voiceDesc: string;
      speed: string;
      speedDesc: string;
      format: string;
      formatDesc: string;
    };

    // General Settings
    general: {
      title: string;
      hotkey: string;
      hotkeyDesc: string;
      hotkeyPlaceholder: string;
      pasteBehavior: string;
      pasteBehaviorDesc: string;
      autoPaste: string;
      clipboardMode: string;
      reviewMode: string;
      disableTextSelection: string;
      disableTextSelectionDesc: string;
      enableDebugLogging: string;
      enableDebugLoggingDesc: string;
      copyDelay: string;
      copyDelayDesc: string;
    };
  };

  // About
  about: {
    title: string;
    version: string;
    description: string;
    features: string;
    featureList: string[];
    credits: string;
    developers: string;
    institution: string;
    repository: string;
    viewOnGithub: string;
  };

  // Review Modal
  review: {
    title: string;
    edit: string;
    copy: string;
    paste: string;
    save: string;
    back: string;
    copied: string;
    pasted: string;
    saved: string;
    imageGenerated: string;
    saveImage: string;
  };

  // Audio Upload
  audio: {
    title: string;
    dragDrop: string;
    browse: string;
    supported: string;
    maxSize: string;
  };

  // Toasts
  toast: {
    success: string;
    error: string;
    warning: string;
    info: string;
    configSaved: string;
    configSaveFailed: string;
    connectionSuccess: string;
    connectionFailed: string;
  };

  // Common
  common: {
    loading: string;
    close: string;
    confirm: string;
    yes: string;
    no: string;
    required: string;
  };

  // History
  history: {
    title: string;
    search: string;
    searchPlaceholder: string;
    noHistory: string;
    noResults: string;
    rerun: string;
    delete: string;
    clearAll: string;
    confirmDelete: string;
    confirmClearAll: string;
    deleted: string;
    cleared: string;
    task: string;
    date: string;
    prompt: string;
    response: string;
  };

  // Custom Tasks
  customTasks: {
    title: string;
    create: string;
    edit: string;
    delete: string;
    confirmDelete: string;
    noTasks: string;
    name: string;
    namePlaceholder: string;
    description: string;
    descriptionPlaceholder: string;
    systemPrompt: string;
    systemPromptPlaceholder: string;
    systemPromptHelp: string;
    options: string;
    addOption: string;
    removeOption: string;
    optionName: string;
    optionNamePlaceholder: string;
    optionType: string;
    optionTypeSelect: string;
    optionTypeText: string;
    optionTypeNumber: string;
    optionRequired: string;
    optionValues: string;
    optionValuesPlaceholder: string;
    optionDefault: string;
    optionMin: string;
    optionMax: string;
    optionStep: string;
    save: string;
    cancel: string;
    validationError: string;
    placeholderMismatch: string;
    exportTasks: string;
    importTasks: string;
    exported: string;
    imported: string;
    importFailed: string;
  };
}

export const translations: Record<Language, Translations> = {
  'en': {
    appName: 'AI Anywhere',

    nav: {
      home: 'Home',
      history: 'History',
      customTasks: 'My Tasks',
      settings: 'Settings',
      apiSettings: 'API Settings',
      appSettings: 'Settings',
      languageSettings: 'Language Settings',
      audioSettings: 'Audio Settings',
      about: 'About',
    },

    home: {
      title: 'AI Anywhere',
      taskSelection: 'Task Selection',
      promptContent: 'Prompt Content',
      clear: 'Clear',
      send: 'Send',
      cancel: 'Cancel',
      processing: 'Processing...',
      enterPrompt: 'Enter your prompt here... (Ctrl+Enter to send)',
      customTasks: 'My Tasks',
      generating: 'Generating...',
      waitingForResponse: 'Waiting for response...',
      promptHint: '💡 Tip: Use Ctrl+Enter to send the prompt.',
    },

    operations: {
      customTask: 'Custom Task',
      customTaskDesc: 'Flexible AI help for any task or question. The prompt is all yours...',
      emailReply: 'Email Reply',
      emailReplyDesc: 'Generate professional email replies',
      imageGeneration: 'Image Generation',
      imageGenerationDesc: 'Generate images with AI',
      speechToText: 'Speech-to-Text (STT)',
      speechToTextDesc: 'Convert audio files to text',
      textToSpeech: 'Text-to-Speech (TTS)',
      textToSpeechDesc: 'Convert text to audio',
      textRewrite: 'Text Rewrite',
      textRewriteDesc: 'Improve and polish text',
      textTranslation: 'Text Translation',
      textTranslationDesc: 'Translate text to another language',
      textSummarization: 'Text Summarization',
      textSummarizationDesc: 'Create concise summaries',
      whatsAppResponse: 'WhatsApp Response',
      whatsAppResponseDesc: 'Generate casual message replies',
      unicodeSymbols: 'Unicode Symbols',
      unicodeSymbolsDesc: 'Find Unicode symbols and emojis',
    },

    settings: {
      title: 'Settings',
      save: 'Save',
      cancel: 'Cancel',
      resetDefaults: 'Reset to Defaults',

      api: {
        title: 'API Settings',
        configuration: 'API Configuration',
        endpoint: 'API Endpoint',
        endpointPlaceholder: 'https://api.openai.com/v1',
        apiKey: 'API Key',
        apiKeyPlaceholder: 'Enter your API key',
        show: 'Show',
        hide: 'Hide',
        apiKeyHelp: 'Your API key is encrypted and stored locally and never sent to external servers except for API calls',
        textModel: 'Text Model',
        imageModel: 'Image Model',
        audioModel: 'Speech-to-Text Model',
        ttsModel: 'Text-to-Speech Model',
        refreshModels: 'Refresh Models',
        modelsAvailable: 'models available',
        testConnection: 'Test API Connectivity',
        testSuccess: 'Connection successful!',
        testFailed: 'Connection failed',
        saveApiSettings: 'Save API Settings',
      },

      language: {
        title: 'Language Settings',
        appLanguage: 'Application Language',
        appLanguageDesc: 'Choose the language for the user interface',
        translationTarget: 'Default Translation Language',
        translationTargetDesc: 'Default target language for translations',
      },

      models: {
        title: 'Models',
      },

      audio: {
        title: 'Audio Settings',
        voice: 'Voice',
        voiceDesc: 'Choose the voice for text-to-speech',
        speed: 'Speed',
        speedDesc: 'Adjust the speech speed (0.25x - 4.0x)',
        format: 'Format',
        formatDesc: 'Output audio format',
      },

      general: {
        title: 'General Settings',
        hotkey: 'Global Hotkey',
        hotkeyDesc: 'Click and press key combination',
        hotkeyPlaceholder: 'Ctrl+Space',
        pasteBehavior: 'Paste Behavior',
        pasteBehaviorDesc: 'How to handle AI responses',
        autoPaste: 'Auto Paste (Paste directly)',
        clipboardMode: 'Clipboard Mode (Copy to clipboard)',
        reviewMode: 'Review Mode (Show preview window)',
        disableTextSelection: 'Disable automatic text selection and clipboard detection',
        disableTextSelectionDesc: 'Enabling this feature makes the app more responsive but less productive',
        enableDebugLogging: 'Enable debug logging for API requests',
        enableDebugLoggingDesc: 'Logs API requests/responses to help diagnose custom endpoint compatibility issues',
        copyDelay: 'Clipboard Delay (ms)',
        copyDelayDesc: 'Time waited before relying on the copied content in the clipboard.',
      },
    },

    about: {
      title: 'About',
      version: 'Version',
      description: 'AI Anywhere is a universal AI assistant that works with any application. Select text, press the hotkey, and get AI-powered help instantly.',
      features: 'Features',
      featureList: [
        'Works with any application',
        'Global hotkey activation',
        'Multiple AI operations',
        'Image generation',
        'Speech-to-text and text-to-speech',
        'Custom API endpoint support',
        'Cross-platform (Windows, macOS, Linux)',
      ],
      credits: 'Credits',
      developers: 'Developed by',
      institution: 'LABiA-FUP/UnB',
      repository: 'Repository',
      viewOnGithub: 'View on GitHub',
    },

    review: {
      title: 'Review Response',
      edit: 'Edit',
      copy: 'Copy',
      paste: 'Paste',
      save: 'Save',
      back: 'Back',
      copied: 'Copied to clipboard',
      pasted: 'Pasted to application',
      saved: 'Saved successfully',
      imageGenerated: 'Image Generated',
      saveImage: 'Save',
    },

    audio: {
      title: 'Audio File',
      dragDrop: 'Drag and drop an audio file here, or',
      browse: 'Browse Files',
      supported: 'Supported formats',
      maxSize: 'Max size',
    },

    toast: {
      success: 'Success',
      error: 'Error',
      warning: 'Warning',
      info: 'Info',
      configSaved: 'Configuration saved successfully',
      configSaveFailed: 'Failed to save configuration',
      connectionSuccess: 'API connection successful',
      connectionFailed: 'API connection failed',
    },

    common: {
      loading: 'Loading...',
      close: 'Close',
      confirm: 'Confirm',
      yes: 'Yes',
      no: 'No',
      required: 'Required',
    },

    history: {
      title: 'History',
      search: 'Search',
      searchPlaceholder: 'Search by prompt or response...',
      noHistory: 'No history entries yet',
      noResults: 'No matching entries found',
      rerun: 'Re-run',
      delete: 'Delete',
      clearAll: 'Clear All',
      confirmDelete: 'Are you sure you want to delete this entry?',
      confirmClearAll: 'Are you sure you want to clear all history? This cannot be undone.',
      deleted: 'Entry deleted',
      cleared: 'History cleared',
      task: 'Task',
      date: 'Date',
      prompt: 'Prompt',
      response: 'Response',
    },

    customTasks: {
      title: 'My Tasks',
      create: 'Create Task',
      edit: 'Edit',
      delete: 'Delete',
      confirmDelete: 'Are you sure you want to delete this task?',
      noTasks: 'No custom tasks yet. Create one to get started!',
      name: 'Task Name',
      namePlaceholder: 'Enter task name...',
      description: 'Description',
      descriptionPlaceholder: 'Enter task description...',
      systemPrompt: 'System Prompt',
      systemPromptPlaceholder: 'Enter the system prompt template...',
      systemPromptHelp: 'Use {placeholder} syntax to reference option values',
      options: 'Options',
      addOption: 'Add Option',
      removeOption: 'Remove',
      optionName: 'Name',
      optionNamePlaceholder: 'option_name',
      optionType: 'Type',
      optionTypeSelect: 'Select',
      optionTypeText: 'Text',
      optionTypeNumber: 'Number',
      optionRequired: 'Required',
      optionValues: 'Values',
      optionValuesPlaceholder: 'value1, value2, value3',
      optionDefault: 'Default',
      optionMin: 'Min',
      optionMax: 'Max',
      optionStep: 'Step',
      save: 'Save Task',
      cancel: 'Cancel',
      validationError: 'Please fix the validation errors',
      placeholderMismatch: 'System prompt must contain all option placeholders',
      exportTasks: 'Export Tasks',
      importTasks: 'Import Tasks',
      exported: 'Tasks exported successfully',
      imported: 'Tasks imported successfully',
      importFailed: 'Failed to import tasks',
    },
  },

  'pt-BR': {
    appName: 'AI Anywhere',

    nav: {
      home: 'Início',
      history: 'Histórico',
      customTasks: 'Minhas Tarefas',
      settings: 'Configurações',
      apiSettings: 'Configurações de API',
      appSettings: 'Configurações do Aplicativo',
      languageSettings: 'Configurações de Idioma',
      audioSettings: 'Configurações de Áudio',
      about: 'Sobre',
    },

    home: {
      title: 'AI Anywhere',
      taskSelection: 'Seleção de Tarefa',
      promptContent: 'Conteúdo do Prompt',
      clear: 'Limpar',
      send: 'Enviar',
      cancel: 'Cancelar',
      processing: 'Processando...',
      enterPrompt: 'Digite seu prompt aqui... (Ctrl+Enter para enviar)',
      customTasks: 'Minhas Tarefas',
      generating: 'Gerando...',
      waitingForResponse: 'Aguardando resposta...',
      promptHint: '💡 Dica: Use Ctrl+Enter para enviar o prompt.',
    },

    operations: {
      customTask: 'Tarefa Personalizada',
      customTaskDesc: 'Ajuda personalizada de IA para qualquer tarefa ou pergunta. O prompt é todo seu...',
      emailReply: 'Resposta de E-mail',
      emailReplyDesc: 'Gere respostas profissionais de e-mail',
      imageGeneration: 'Geração de Imagem',
      imageGenerationDesc: 'Gere imagens com IA',
      speechToText: 'Fala para Texto (STT)',
      speechToTextDesc: 'Converta arquivos de áudio em texto',
      textToSpeech: 'Texto para Fala (TTS)',
      textToSpeechDesc: 'Converta texto em áudio',
      textRewrite: 'Reescrita de Texto',
      textRewriteDesc: 'Melhore e refine textos',
      textTranslation: 'Tradução de Texto',
      textTranslationDesc: 'Traduza texto para outro idioma',
      textSummarization: 'Resumo de Texto',
      textSummarizationDesc: 'Crie resumos concisos',
      whatsAppResponse: 'Resposta WhatsApp',
      whatsAppResponseDesc: 'Gere respostas casuais para mensagens',
      unicodeSymbols: 'Símbolos Unicode',
      unicodeSymbolsDesc: 'Encontre símbolos Unicode e emojis',
    },

    settings: {
      title: 'Configurações',
      save: 'Salvar',
      cancel: 'Cancelar',
      resetDefaults: 'Restaurar Padrões',

      api: {
        title: 'Configurações de API',
        configuration: 'Configuração da API',
        endpoint: 'Endpoint da API',
        endpointPlaceholder: 'https://api.openai.com/v1',
        apiKey: 'Chave da API',
        apiKeyPlaceholder: 'Digite sua chave de API',
        show: 'Mostrar',
        hide: 'Ocultar',
        apiKeyHelp: 'Sua chave de API é encriptada e armazenada localmente, nunca é enviada para servidores externos, exceto para chamadas de API',
        textModel: 'Modelo de Texto',
        imageModel: 'Modelo de Imagem',
        audioModel: 'Modelo de Fala para Texto',
        ttsModel: 'Modelo de Texto para Fala',
        refreshModels: 'Atualizar Modelos',
        modelsAvailable: 'modelos disponíveis',
        testConnection: 'Testar Conectividade da API',
        testSuccess: 'Conexão bem-sucedida!',
        testFailed: 'Falha na conexão',
        saveApiSettings: 'Salvar Configurações de API',
      },

      language: {
        title: 'Configurações de Idioma',
        appLanguage: 'Idioma da Aplicação',
        appLanguageDesc: 'Escolha o idioma da interface do usuário',
        translationTarget: 'Idioma de Tradução Padrão',
        translationTargetDesc: 'Idioma padrão de destino para traduções',
      },

      models: {
        title: 'Modelos',
      },

      audio: {
        title: 'Configurações de Áudio',
        voice: 'Voz',
        voiceDesc: 'Escolha a voz para texto para fala',
        speed: 'Velocidade',
        speedDesc: 'Ajuste a velocidade da fala (0.25x - 4.0x)',
        format: 'Formato',
        formatDesc: 'Formato de saída do áudio',
      },

      general: {
        title: 'Configurações Gerais',
        hotkey: 'Atalho Global',
        hotkeyDesc: 'Clique e pressione a combinação de teclas',
        hotkeyPlaceholder: 'Ctrl+Space',
        pasteBehavior: 'Comportamento de Colagem',
        pasteBehaviorDesc: 'Como lidar com respostas da IA',
        autoPaste: 'Colar Automaticamente (Cola diretamente)',
        clipboardMode: 'Modo Área de Transferência (Copia para área de transferência)',
        reviewMode: 'Modo Revisão (Mostra janela de pré-visualização)',
        disableTextSelection: 'Desabilitar seleção automática de texto e detecção de área de transferência',
        disableTextSelectionDesc: 'Habilitar este recurso torna o app mais responsivo, mas menos produtivo',
        enableDebugLogging: 'Habilitar registro de depuração para requisições de API',
        enableDebugLoggingDesc: 'Registra requisições/respostas de API para ajudar a diagnosticar problemas de compatibilidade de endpoints personalizados',
        copyDelay: 'Atraso de Clipboard (ms)',
        copyDelayDesc: 'Tempo se espera antes de contar com o conteúdo copiado para a área de transferência.',
      },
    },

    about: {
      title: 'Sobre',
      version: 'Versão',
      description: 'AI Anywhere é um assistente de IA universal que funciona com qualquer aplicação. Selecione texto, pressione a tecla de atalho e obtenha ajuda com IA instantaneamente.',
      features: 'Recursos',
      featureList: [
        'Funciona com qualquer aplicação',
        'Ativação por tecla de atalho global',
        'Múltiplas operações de IA',
        'Geração de imagens',
        'Fala para texto e texto para fala',
        'Suporte a endpoints de API personalizados',
        'Multiplataforma (Windows, macOS, Linux)',
      ],
      credits: 'Créditos',
      developers: 'Desenvolvido por',
      institution: 'LABiA-FUP/UnB',
      repository: 'Repositório',
      viewOnGithub: 'Ver no GitHub',
    },

    review: {
      title: 'Revisar Resposta',
      edit: 'Editar',
      copy: 'Copiar',
      paste: 'Colar',
      save: 'Salvar',
      back: 'Voltar',
      copied: 'Copiado para a área de transferência',
      pasted: 'Colado na aplicação',
      saved: 'Salvo com sucesso',
      imageGenerated: 'Imagem Gerada',
      saveImage: 'Salvar',
    },

    audio: {
      title: 'Arquivo de Áudio',
      dragDrop: 'Arraste e solte um arquivo de áudio aqui, ou',
      browse: 'Procurar Arquivos',
      supported: 'Formatos suportados',
      maxSize: 'Tamanho máximo',
    },

    toast: {
      success: 'Sucesso',
      error: 'Erro',
      warning: 'Aviso',
      info: 'Informação',
      configSaved: 'Configuração salva com sucesso',
      configSaveFailed: 'Falha ao salvar configuração',
      connectionSuccess: 'Conexão com API bem-sucedida',
      connectionFailed: 'Falha na conexão com API',
    },

    common: {
      loading: 'Carregando...',
      close: 'Fechar',
      confirm: 'Confirmar',
      yes: 'Sim',
      no: 'Não',
      required: 'Obrigatório',
    },

    history: {
      title: 'Histórico',
      search: 'Buscar',
      searchPlaceholder: 'Buscar por prompt ou resposta...',
      noHistory: 'Nenhuma entrada no histórico ainda',
      noResults: 'Nenhuma entrada encontrada',
      rerun: 'Reexecutar',
      delete: 'Excluir',
      clearAll: 'Limpar Tudo',
      confirmDelete: 'Tem certeza que deseja excluir esta entrada?',
      confirmClearAll: 'Tem certeza que deseja limpar todo o histórico? Esta ação não pode ser desfeita.',
      deleted: 'Entrada excluída',
      cleared: 'Histórico limpo',
      task: 'Tarefa',
      date: 'Data',
      prompt: 'Prompt',
      response: 'Resposta',
    },

    customTasks: {
      title: 'Minhas Tarefas',
      create: 'Criar Tarefa',
      edit: 'Editar',
      delete: 'Excluir',
      confirmDelete: 'Tem certeza que deseja excluir esta tarefa?',
      noTasks: 'Nenhuma tarefa personalizada ainda. Crie uma para começar!',
      name: 'Nome da Tarefa',
      namePlaceholder: 'Digite o nome da tarefa...',
      description: 'Descrição',
      descriptionPlaceholder: 'Digite a descrição da tarefa...',
      systemPrompt: 'Prompt do Sistema',
      systemPromptPlaceholder: 'Digite o modelo do prompt do sistema...',
      systemPromptHelp: 'Use a sintaxe {placeholder} para referenciar valores de opções',
      options: 'Opções',
      addOption: 'Adicionar Opção',
      removeOption: 'Remover',
      optionName: 'Nome',
      optionNamePlaceholder: 'nome_opcao',
      optionType: 'Tipo',
      optionTypeSelect: 'Seleção',
      optionTypeText: 'Texto',
      optionTypeNumber: 'Número',
      optionRequired: 'Obrigatório',
      optionValues: 'Valores',
      optionValuesPlaceholder: 'valor1, valor2, valor3',
      optionDefault: 'Padrão',
      optionMin: 'Mín',
      optionMax: 'Máx',
      optionStep: 'Passo',
      save: 'Salvar Tarefa',
      cancel: 'Cancelar',
      validationError: 'Por favor, corrija os erros de validação',
      placeholderMismatch: 'O prompt do sistema deve conter todos os placeholders das opções',
      exportTasks: 'Exportar Tarefas',
      importTasks: 'Importar Tarefas',
      exported: 'Tarefas exportadas com sucesso',
      imported: 'Tarefas importadas com sucesso',
      importFailed: 'Falha ao importar tarefas',
    },
  },
};
