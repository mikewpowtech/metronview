import { Resizable } from "react-resizable";
import "react-resizable/css/styles.css";

const ResizableTitle = (props: any) => {
    const { onResize, width, ...restProps } = props;
    if (!width) {
        return <th {...restProps} />;
    }
    return (
        <Resizable
            width={width}
            height={0}
            handle={
                <span
                    className="react-resizable-handle"
                    style={{
                        position: "absolute",
                        right: 0,
                        top: 0,
                        height: "100%",
                        width: 2,
                        cursor: "ew-resize",
                        zIndex: 2,
                        userSelect: "none",
                        background: "rgba(0,0,0,0.08)",
                        transition: "background 0.2s",
                    }}
                    onMouseOver={e => (e.currentTarget.style.background = "rgba(0,0,0,0.18)")}
                    onMouseOut={e => (e.currentTarget.style.background = "rgba(0,0,0,0.08)")}
                    onClick={e => e.stopPropagation()}
                />
            }
            onResize={onResize}
            draggableOpts={{ enableUserSelectHack: false }}
        >
            <th {...restProps} style={{ position: "relative", ...restProps.style }} />
           </Resizable>
       );
   };

   export default ResizableTitle;