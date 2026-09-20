open System.Numerics
open FsToolbox.GLTF
open SharpGLTF.Schema2

type TranslationCurveValue = { Time: float32; Value: Vector3 }

type RotationCurveValue = { Time: float32; Value: Quaternion }

type ScaleCurveValue = { Time: float32; Value: Vector3 }

type Channel =
    { NodeId: int
      TranslationCurve: TranslationCurveValue list
      RotationCurve: RotationCurveValue list
      ScaleCurve: ScaleCurveValue list }

type AnimationClip =
    { Name: string
      Duration: float32
      Channels: Channel list }

type Armature = { RootBone: Bone }

and Bone = { Name: string; InverseBindMatrix: Matrix4x4; Children: Bone list }



let vl =
    GLTFLoader.loadRoot "/home/maxc/Projects/blender/low_poly_male_rigged.gltf"

//let rootNode =

let buildArmature (vl: ModelRoot) =

    [ for skin in vl.LogicalSkins do
          let joints = skin.Joints
          

          let root =
              joints
              |> Seq.find (fun j ->
                  let hasParent =
                      joints |> Seq.exists (fun other -> other.VisualChildren |> Seq.contains j)

                  hasParent |> not)
              
          let i = skin.InverseBindMatrices

          let rec build (node: Node) =
              
              let iv = i.[node.LogicalIndex]

              { Name = node.Name
                InverseBindMatrix = iv
                Children = node.VisualChildren |> Seq.map build |> List.ofSeq }

          { RootBone = build root } ]

let buildAnimations (vl: ModelRoot) =
    [ for animation in vl.LogicalAnimations do
          { Name = animation.Name
            Duration = animation.Duration
            Channels =
              [ for (nodeId, channels) in animation.Channels |> Seq.groupBy (fun c -> c.TargetNode.LogicalIndex) do
                    let tc, rc, sc =
                        channels
                        |> Seq.fold
                            (fun
                                (tc: TranslationCurveValue list option,
                                 rc: RotationCurveValue list option,
                                 sc: ScaleCurveValue list option)
                                c ->
                                let newTc =
                                    match c.GetTranslationSampler() with
                                    | null -> tc
                                    | sampler ->
                                        [ for time, v in sampler.GetLinearKeys() do
                                              { Time = time; Value = v }: TranslationCurveValue ]
                                        |> Some

                                let newRc =
                                    match c.GetRotationSampler() with
                                    | null -> rc
                                    | sampler ->
                                        [ for time, v in sampler.GetLinearKeys() do
                                              ({ Time = time; Value = v }: RotationCurveValue) ]
                                        |> Some

                                let newSc =
                                    match c.GetScaleSampler() with
                                    | null -> sc
                                    | sampler ->
                                        [ for time, v in sampler.GetLinearKeys() do
                                              ({ Time = time; Value = v }: ScaleCurveValue) ]
                                        |> Some

                                (newTc, newRc, newSc))
                            (None, None, None)

                    { NodeId = nodeId
                      TranslationCurve = tc |> Option.defaultValue []
                      RotationCurve = rc |> Option.defaultValue []
                      ScaleCurve = sc |> Option.defaultValue [] } ] } ]

(*

for node in vl.LogicalNodes do
    node.GetCurveSamplers()



// animations
for animation in vl.LogicalAnimations do


    // Channels
    for channel in animation.Channels do
        channel.TargetNode.LogicalIndex

        match channel.GetTranslationSampler() with
        | null -> ()
        | sampler ->
            for time, v in sampler.GetLinearKeys() do
                ()

        match channel.GetRotationSampler() with
        | null -> ()
        | sampler ->
            for time, v in sampler.GetLinearKeys() do
                ()

        match channel.GetScaleSampler() with
        | null -> ()
        | sampler ->
            for time, v in sampler.GetLinearKeys() do
                ()




        ()
*)

let m = GLTFLoader.loadModel "/home/maxc/Projects/blender/low_poly_male_rigged.gltf"

let s = buildArmature vl

let anims = buildAnimations vl

let i = anims.[1].Channels |> List.sortBy (fun l -> l.NodeId)



// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"
