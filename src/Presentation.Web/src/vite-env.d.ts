/// <reference types="vite/client" />
/// <reference types="react" />
/// <reference types="react-dom" />

// Type declarations for module resolution
declare module "*.scss" {
  const content: Record<string, string>;
  export default content;
}

declare module "*.css" {
  const content: Record<string, string>;
  export default content;
}

declare module "*.svg" {
  const content: any;
  export default content;
}

declare module "*.png" {
  const content: any;
  export default content;
}

declare module "*.jpg" {
  const content: any;
  export default content;
}

declare module "*.jpeg" {
  const content: any;
  export default content;
}

declare module "*.gif" {
  const content: any;
  export default content;
}

declare module "*.webp" {
  const content: any;
  export default content;
}

declare module "*.ico" {
  const content: any;
  export default content;
}

declare module "*.woff" {
  const content: any;
  export default content;
}

declare module "*.woff2" {
  const content: any;
  export default content;
}

declare module "*.eot" {
  const content: any;
  export default content;
}

declare module "*.ttf" {
  const content: any;
  export default content;
}

declare module "*.otf" {
  const content: any;
  export default content;
}
