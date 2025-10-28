# Documentation Guidelines

Guidelines for maintaining comprehensive documentation for the EFD game system.

## 📋 Documentation Standards

### When to Document
Document the following scenarios:
- **New Features**: Any new system or major functionality
- **Feature Updates**: Significant changes to existing systems
- **Bug Fixes**: Important fixes that change behavior
- **API Changes**: Changes to public methods or interfaces
- **Configuration Changes**: New settings or modified workflows

### Documentation Requirements
Each feature should include:
- **Overview**: What it does and why
- **Setup Instructions**: How to implement/use
- **Code Examples**: Practical usage samples
- **Testing Information**: How to test and validate
- **Integration Details**: How it works with other systems

## 📁 File Organization

### Documentation Structure
```
Assets/Scripts/Documentation/
├── README.md                    # Main index and overview
├── FeatureTemplate.md          # Template for new features
├── DocumentationGuidelines.md  # This file
├── HealthSystem.md             # Individual feature docs
├── WeaponSystem.md
├── PlayerController.md
├── TestingTools.md
├── ScriptableObjects.md
└── DamageTagsSystem.md
```

### Naming Conventions
- Use PascalCase for file names: `HealthSystem.md`
- Use descriptive names that match the feature
- Include version numbers for major changes
- Use consistent headings and formatting

## ✍️ Writing Standards

### Formatting Guidelines
Use consistent markdown formatting:

```markdown
# Main Title (H1 - Feature Name)
## Section Title (H2 - Major Sections)
### Subsection (H3 - Details)

**Bold** for emphasis
*Italic* for notes
`Code` for inline code
```

### Code Examples
Always include practical code examples:

```csharp
// Good: Complete, runnable example
public class ExampleUsage : MonoBehaviour
{
    void Start()
    {
        Health health = GetComponent<Health>();
        health.TakeDamage(10);
    }
}

// Avoid: Incomplete fragments without context
health.TakeDamage(10);
```

### Section Templates
Use consistent section structures:

#### Required Sections
- **Overview**: What and why
- **Setup Instructions**: How to implement
- **Configuration Options**: Available settings
- **Integration**: How it works with other systems

#### Optional Sections
- **Advanced Usage**: Complex scenarios
- **Known Issues**: Current limitations
- **Future Enhancements**: Planned improvements
- **Change Log**: Version history

## 🔄 Update Workflow

### For New Features
1. **Create Feature**: Implement the functionality
2. **Document Immediately**: Create documentation while fresh
3. **Use Template**: Start with `FeatureTemplate.md`
4. **Update Index**: Add to main `README.md`
5. **Cross-Reference**: Link from related documentation

### For Feature Updates
1. **Update Existing Docs**: Modify relevant documentation
2. **Add Change Log Entry**: Note what changed and when
3. **Update Code Examples**: Ensure examples still work
4. **Review Cross-References**: Update links in other docs

### Documentation Review Process
Before finalizing documentation:
- [ ] All sections complete
- [ ] Code examples tested
- [ ] Links work correctly
- [ ] Formatting consistent
- [ ] Index updated

## 📝 Content Guidelines

### Writing Style
- **Clear and Concise**: Avoid unnecessary complexity
- **Action-Oriented**: Use active voice and clear instructions
- **User-Focused**: Write from the developer's perspective
- **Consistent Terminology**: Use the same terms throughout

### Code Documentation
```csharp
/// <summary>
/// Brief description of what this method does
/// </summary>
/// <param name="amount">Description of parameter</param>
/// <returns>Description of return value</returns>
public bool TakeDamage(int amount)
{
    // Implementation
}
```

### Visual Elements
Use consistent visual elements:
- 📊 for overviews
- 🔧 for setup/configuration
- 🎯 for specific instructions
- 🧪 for testing
- 🐛 for issues/bugs
- 🔄 for future plans
- ⚙️ for settings/options

## 🔗 Cross-Referencing

### Linking Between Documents
Always link related systems:
```markdown
The Health System integrates with the [Weapon System](./WeaponSystem.md) 
for damage application and with the [Player Controller](./PlayerController.md) 
for player health management.
```

### Maintaining Link Integrity
- Use relative paths for internal links
- Check links when moving or renaming files
- Update all references when changing file names

## 📋 Quality Checklist

### Before Publishing Documentation
- [ ] **Accuracy**: All information is correct and up-to-date
- [ ] **Completeness**: All required sections included
- [ ] **Clarity**: Instructions are easy to follow
- [ ] **Examples**: Code examples are tested and working
- [ ] **Links**: All cross-references work correctly
- [ ] **Formatting**: Consistent markdown formatting
- [ ] **Index**: Main README updated with new content

### Periodic Review
Schedule regular documentation reviews:
- **Monthly**: Check for outdated information
- **Per Release**: Update all affected documentation
- **When Issues Arise**: Document solutions and workarounds

## 🛠️ Tools & Automation

### Recommended Tools
- **Markdown Editor**: Typora, Mark Text, or VS Code
- **Link Checker**: Automated link validation
- **Version Control**: Git for tracking documentation changes

### Automation Opportunities
Future improvements could include:
- Automated link checking
- Documentation generation from code comments
- Template validation scripts
- Automatic table of contents generation

## 📊 Documentation Metrics

### Success Indicators
Track documentation effectiveness:
- **Usage**: How often docs are referenced
- **Completeness**: Percentage of features documented
- **Accuracy**: Frequency of corrections needed
- **Feedback**: Developer comments on usefulness

### Maintenance Schedule
- **Weekly**: Update active development documentation
- **Monthly**: Review and update existing docs
- **Quarterly**: Major documentation restructuring if needed
- **Per Release**: Comprehensive review of all documentation

---
*Last Updated: October 28, 2025*