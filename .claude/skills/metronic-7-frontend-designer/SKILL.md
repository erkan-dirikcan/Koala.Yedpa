---
name: metronic-7-frontend-designer
description: Senior-level Metronic 7 theme expert for ASP.NET Core applications. Specializes in page structures, JavaScript/CSS architecture, and partial view development. Use when building frontend pages with Metronic 7 theme: creating new pages, implementing layouts, adding JavaScript functionality, working with SCSS customization, or handling authentication pages (Login/Register/Forgot Password). Ask for assets folder path and copy assets to wwwroot before starting.
---

# Metronic 7 FrontEnd Designer

Expert-level assistant for building Metronic 7 themed frontend pages in ASP.NET Core applications.

## Initial Setup

Before creating any pages, always:

1. **Ask for the Metronic assets folder path** - The source location of Metronic theme files
2. **Ask for logo and favicon paths** - Required for branding
3. **Copy assets to wwwroot** - Move assets from source to `wwwroot/assets/`
4. **Copy SCSS files** - Extract SCSS files from template sass folder to project
5. **Ask for project namespace** - Required for ManageNavPages class

## Project Structure

```
wwwroot/
├── assets/
│   ├── js/
│   │   └── custom/          # All custom JavaScript files go here
│   ├── css/                 # Compiled CSS
│   ├── media/               # Images, fonts
│   └── sass/                # SCSS source from template
```

## Page Creation Workflow

### 1. Create Controller and Views

Create controllers and views following ASP.NET Core MVC conventions. Place views in `Views/{ControllerName}/`.

### 2. JavaScript Implementation

**NEVER write JavaScript/jQuery inline in views.** Always create separate files in `wwwroot/assets/js/custom/`.

Use this template for custom JavaScript:

```javascript
"use strict";
var KL[InitName] = function() {

    var [init1] = function() {
        // Initialization code
    };

    var [init2] = function() {
        // More initialization
    };

    return {
        init: function() {
            [init1]();
            [init2]();
        }
    };
}();

jQuery(document).ready(function() {
    KL[InitName].init();
});
```

Replace placeholders:
- `[InitName]` - Descriptive name for the page/feature
- `[init1]`, `[init2]` - Function names for initialization steps

Reference: `assets/templates/javascript-template.js`

### 3. ManageNavPages Integration

Create/Update `ManageNavPages.cs` for menu active states. Add each new button/page to this class.

Reference: `assets/templates/ManageNavPages.cs.txt`

**Menu Type Patterns:**

**Single Menu Item:**
```csharp
private static string PageName => "PageName";
public static string PageNameNavClass(ViewContext viewContext) => PageMainNavClass(viewContext, PageName);
```

**Parent Menu with Children:**
```csharp
// Parent
private static string ParentMenu => "ParentMenu";
public static string ParentMenuNavClass(ViewContext viewContext) => PageMainToogleNavClass(viewContext, ParentMenu);

// Children
private static string ChildPage1 => "ChildPage1";
private static string ChildPage2 => "ChildPage2";
public static string ChildPage1NavClass(ViewContext viewContext) => PageMainNavClass(viewContext, ChildPage1);
public static string ChildPage2NavClass(ViewContext viewContext) => PageMainNavClass(viewContext, ChildPage2);
```

### 4. Partial Views

Use `_MainMenuPartial.cshtml` template for sidebar menu. Replace `{LogoPath}` with actual logo path.

Reference: `assets/templates/_MainMenuPartial.cshtml.txt`

In Controllers, set active page:
```csharp
ViewData["ActivePage"] = ManageNavPages.PageName;
ViewData["MenuToggle"] = ManageNavPages.ParentMenu; // For child items
```

### 5. Authentication Pages

For Login, Register, Forgot Password pages, use the template at:
`D:/Template/Metronic-Metronic-v7/theme/html/demo1/dist/custom/pages/login/login-1.html`

## CSS/SCSS Customization

SCSS source files location: `D:\Template\Metronic-Metronic-v7\theme\html\demo1\src\sass`

Copy relevant SCSS files to project and modify for:
- Color palette updates
- Custom styles
- Theme overrides

## Ajax/JavaScript Interactions

For AJAX operations, form submissions, and dynamic content:

1. Create init function in custom JavaScript file
2. Use jQuery for AJAX (already loaded by Metronic)
3. Handle responses with appropriate UI updates
4. Use Metronic's notification system for feedback

**Example AJAX pattern:**
```javascript
var handleSubmit = function() {
    $('#formId').submit(function(e) {
        e.preventDefault();
        var formData = $(this).serialize();

        $.ajax({
            url: $(this).attr('action'),
            type: 'POST',
            data: formData,
            success: function(response) {
                // Handle success
            },
            error: function() {
                // Handle error
            }
        });
    });
};
```

## Template Reference

For complete class reference, layouts, and component structure, see: [structure.md](references/structure.md)

Key sections:
- Metronic 7 CSS classes for all components
- Project structure conventions
- Icon usage (Keenicons, Flaticon, SVG)

## Key Principles

1. **No inline JavaScript** - Always use external files in `wwwroot/assets/js/custom/`
2. **Consistent naming** - Use KL prefix for JavaScript objects
3. **Menu management** - Always add pages to ManageNavPages
4. **Template adherence** - Follow Metronic 7 demo1 structure
5. **Proper asset organization** - Keep custom code separate from theme files
