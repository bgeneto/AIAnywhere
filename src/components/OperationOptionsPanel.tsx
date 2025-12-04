import { useApp } from '../context/AppContext';
import { useI18n } from '../i18n';
import { OperationOption } from '../types';

// Mapping from option keys to translation keys
const optionKeyToTranslationKey: Record<string, string> = {
  'tone': 'tone',
  'length': 'length',
  'language': 'language',
  'size': 'imageSize',
  'quality': 'quality',
  'style': 'style',
  'format': 'format',
  'voice': 'voice',
  'speed': 'speed',
  'model': 'model',
};

// Mapping from option values to translation keys (for tones)
const toneValueToKey: Record<string, keyof typeof import('../i18n/translations').translations['en']['operationOptions']['tones']> = {
  'PROFESSIONAL': 'professional',
  'professional': 'professional',
  'FRIENDLY': 'friendly',
  'friendly': 'friendly',
  'FORMAL': 'formal',
  'formal': 'formal',
  'URGENT': 'urgent',
  'APOLOGETIC': 'apologetic',
  'ENTHUSIASTIC': 'enthusiastic',
  'enthusiastic': 'enthusiastic',
  'academic': 'academic',
  'CASUAL': 'casual',
  'casual': 'casual',
  'creative': 'creative',
  'informal': 'informal',
  'SUPPORTIVE': 'supportive',
  'HUMOROUS': 'humorous',
};

// Mapping for length values
const lengthValueToKey: Record<string, keyof typeof import('../i18n/translations').translations['en']['operationOptions']['lengths']> = {
  'BRIEF': 'brief',
  'brief': 'brief',
  'STANDARD': 'standard',
  'standard': 'standard',
  'DETAILED': 'detailed',
  'detailed': 'detailed',
  'MEDIUM': 'medium',
  'medium': 'medium',
  'SHORT': 'short',
  'LONG': 'long',
};

// Mapping for format values
const formatValueToKey: Record<string, keyof typeof import('../i18n/translations').translations['en']['operationOptions']['formats']> = {
  'paragraph': 'paragraph',
  'bullet points': 'bulletPoints',
  'executive summary': 'executiveSummary',
  'key takeaways': 'keyTakeaways',
};

// Mapping for quality values
const qualityValueToKey: Record<string, keyof typeof import('../i18n/translations').translations['en']['operationOptions']['qualities']> = {
  'standard': 'standard',
  'hd': 'hd',
};

// Mapping for style values
const styleValueToKey: Record<string, keyof typeof import('../i18n/translations').translations['en']['operationOptions']['styles']> = {
  'vivid': 'vivid',
  'natural': 'natural',
};

// Mapping for language values (for STT and TTS language options)
const languageCodeToKey: Record<string, keyof typeof import('../i18n/translations').translations['en']['operationOptions']['languages']> = {
  'auto': 'auto',
  'ar': 'arabic',
  'bn': 'bengali',
  'zh': 'chinese',
  'zh-cn': 'chinese',
  'en': 'english',
  'fr': 'french',
  'de': 'german',
  'hi': 'hindi',
  'it': 'italian',
  'ja': 'japanese',
  'ko': 'korean',
  'pt': 'portuguese',
  'pa': 'punjabi',
  'ru': 'russian',
  'es': 'spanish',
  'pl': 'polish',
  'tr': 'turkish',
  'cs': 'czech',
  'nl': 'dutch',
  // Full names (for translation target language)
  'Arabic': 'arabic',
  'Bengali': 'bengali',
  'Chinese': 'chinese',
  'English': 'english',
  'French': 'french',
  'German': 'german',
  'Hindi': 'hindi',
  'Italian': 'italian',
  'Japanese': 'japanese',
  'Korean': 'korean',
  'Portuguese': 'portuguese',
  'Punjabi': 'punjabi',
  'Russian': 'russian',
  'Spanish': 'spanish',
};

export function OperationOptionsPanel() {
  const { selectedOperation, operationOptions, setOperationOption } = useApp();
  const { t } = useI18n();

  if (!selectedOperation || selectedOperation.options.length === 0) {
    return null;
  }

  // Get translated option name
  const getOptionLabel = (option: OperationOption): string => {
    const translationKey = optionKeyToTranslationKey[option.key];
    if (translationKey && t.operationOptions) {
      // Special case for "Language (optional)" in STT
      if (option.key === 'language' && option.name.includes('optional')) {
        return t.operationOptions.languageOptional;
      }
      // Special cases for specific option names
      if (option.key === 'tone') {
        // Check if it's writing tone or response tone based on parent operation
        if (option.name === 'Writing Tone') {
          return t.operationOptions.writingTone;
        }
        if (option.name === 'Response Tone') {
          return t.operationOptions.responseTone;
        }
        return t.operationOptions.tone;
      }
      if (option.key === 'length') {
        if (option.name === 'Summary Length') {
          return t.operationOptions.summaryLength;
        }
        if (option.name === 'Response Length') {
          return t.operationOptions.responseLength;
        }
        return t.operationOptions.length;
      }
      if (option.key === 'format') {
        if (option.name === 'Output Format') {
          return t.operationOptions.outputFormat;
        }
        return t.operationOptions.format;
      }
      if (option.key === 'size' && option.name === 'Image Size') {
        return t.operationOptions.imageSize;
      }
      if (option.key === 'language' && option.name === 'Target Language') {
        return t.operationOptions.targetLanguage;
      }
      // Safe lookup for string-only properties (skip nested objects like tones, lengths, etc.)
      const value = t.operationOptions[translationKey as keyof typeof t.operationOptions];
      if (typeof value === 'string') {
        return value;
      }
    }
    return option.name;
  };

  // Get translated option value
  const getOptionValueLabel = (option: OperationOption, value: string): string => {
    if (!t.operationOptions) return value;
    
    // Tone values
    if (option.key === 'tone') {
      const toneKey = toneValueToKey[value];
      if (toneKey && t.operationOptions.tones[toneKey]) {
        return t.operationOptions.tones[toneKey];
      }
    }
    
    // Length values
    if (option.key === 'length') {
      const lengthKey = lengthValueToKey[value];
      if (lengthKey && t.operationOptions.lengths[lengthKey]) {
        return t.operationOptions.lengths[lengthKey];
      }
    }
    
    // Format values
    if (option.key === 'format' && option.name !== 'Output Format') {
      const formatKey = formatValueToKey[value];
      if (formatKey && t.operationOptions.formats[formatKey]) {
        return t.operationOptions.formats[formatKey];
      }
    }
    
    // Quality values
    if (option.key === 'quality') {
      const qualityKey = qualityValueToKey[value];
      if (qualityKey && t.operationOptions.qualities[qualityKey]) {
        return t.operationOptions.qualities[qualityKey];
      }
    }
    
    // Style values
    if (option.key === 'style') {
      const styleKey = styleValueToKey[value];
      if (styleKey && t.operationOptions.styles[styleKey]) {
        return t.operationOptions.styles[styleKey];
      }
    }
    
    // Language values (for STT/TTS language options and translation target)
    if (option.key === 'language') {
      const langKey = languageCodeToKey[value];
      if (langKey && t.operationOptions.languages[langKey]) {
        return t.operationOptions.languages[langKey];
      }
    }
    
    return value;
  };

  const renderOption = (option: OperationOption) => {
    const value = operationOptions[option.key] || option.defaultValue;
    const optionLabel = getOptionLabel(option);

    switch (option.type) {
      case 'select':
        return (
          <div key={option.key} className="space-y-1">
            <label 
              htmlFor={option.key}
              className="form-label"
            >
              {optionLabel}
              {option.required && <span className="text-red-500 ml-1">*</span>}
            </label>
            <select
              id={option.key}
              value={value}
              onChange={(e) => setOperationOption(option.key, e.target.value)}
              className="form-input-sm"
            >
              {option.values.map((v) => (
                <option key={v} value={v}>
                  {getOptionValueLabel(option, v)}
                </option>
              ))}
            </select>
          </div>
        );

      case 'text':
        return (
          <div key={option.key} className="space-y-1">
            <label 
              htmlFor={option.key}
              className="form-label"
            >
              {optionLabel}
              {option.required && <span className="text-red-500 ml-1">*</span>}
            </label>
            <input
              type="text"
              id={option.key}
              value={value}
              onChange={(e) => setOperationOption(option.key, e.target.value)}
              placeholder={option.defaultValue}
              className="form-input-sm"
            />
          </div>
        );

      case 'number':
        return (
          <div key={option.key} className="space-y-1">
            <label 
              htmlFor={option.key}
              className="form-label"
            >
              {optionLabel}
              {option.required && <span className="text-red-500 ml-1">*</span>}
            </label>
            <input
              type="number"
              id={option.key}
              value={value}
              onChange={(e) => setOperationOption(option.key, e.target.value)}
              placeholder={option.defaultValue}
              className="form-input-sm"
            />
          </div>
        );

      default:
        return null;
    }
  };

  return (
    <div className="grid grid-cols-2 gap-3">
      {selectedOperation.options.map(renderOption)}
    </div>
  );
}
