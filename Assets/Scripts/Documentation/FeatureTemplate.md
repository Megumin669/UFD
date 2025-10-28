# Feature Documentation Template

Use this template when documenting new features or updates to existing systems.

## 📋 Feature: [Feature Name]

**Version:** [Version/Date Added]  
**Author:** [Developer Name]  
**Status:** [Development/Testing/Complete]

## 📊 Overview

Brief description of what this feature does and why it was added.

**Key Benefits:**
- Benefit 1
- Benefit 2  
- Benefit 3

**Dependencies:**
- Required System 1
- Required System 2

## 🔧 Technical Implementation

### Core Components
List main scripts and components:

#### [ComponentName].cs
Purpose and main functionality.

**Key Methods:**
- `Method1()` - Description
- `Method2()` - Description

**Key Properties:**
- `property1` - Description
- `property2` - Description

### Architecture Decisions
Explain key design choices and why they were made.

## 🎯 Setup Instructions

### Basic Setup
Step-by-step setup for basic usage:

1. **Step 1**: Description
2. **Step 2**: Description
3. **Step 3**: Description

### Advanced Configuration
```csharp
// Code example for advanced setup
public class ExampleUsage : MonoBehaviour
{
    // Configuration example
}
```

## 🎮 Usage Examples

### Basic Usage
```csharp
// Simple usage example
ExampleComponent component = GetComponent<ExampleComponent>();
component.DoSomething();
```

### Advanced Usage
```csharp
// Advanced usage example with callbacks
component.OnSomethingHappened += (data) => {
    // Handle event
};
```

## ⚙️ Configuration Options

### Inspector Settings
- **Setting 1**: Description and range
- **Setting 2**: Description and default value

### Code Configuration
```csharp
// Runtime configuration example
component.ConfigureSetting(value);
```

## 🔔 Events & Integration

### Events Provided
- `OnEventName` - When this event fires and what data it provides

### Integration Points
- **System 1**: How this feature integrates
- **System 2**: Integration details

## 🧪 Testing

### Testing Tools
- **TestingScript.cs**: Description of testing functionality

### Testing Scenarios
1. **Test Case 1**: Description and expected result
2. **Test Case 2**: Description and expected result

### Testing Controls
- **Key 1**: Action description
- **Key 2**: Action description

## 🐛 Known Issues & Limitations

### Current Limitations
- Limitation 1: Description
- Limitation 2: Description

### Known Issues
- Issue 1: Description and workaround
- Issue 2: Description and planned fix

## 🔄 Future Improvements

### Planned Enhancements
- Enhancement 1: Description
- Enhancement 2: Description

### Potential Expansions
- Expansion 1: Description
- Expansion 2: Description

## 📋 Change Log

### [Date] - Version X.X
- Change 1
- Change 2
- Bug Fix 1

---
*Template Version: 1.0*  
*Last Updated: October 28, 2025*